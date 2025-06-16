using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using LabFrame2023;
using Newtonsoft.Json;

public class LabDataManager : LabSingleton<LabDataManager>, IManager
{
    /// <summary>
    /// 遊戲運行資料，可自由呼叫
    /// </summary>
    public LabGameData GameData;

    /// <summary>
    /// LabDataManager 是否已初始化 (是否呼叫過 LabDataInit)
    /// </summary>
    public bool IsInited { get; private set; } = false;

    /// <summary>
    /// 是否接受儲存新的 LabData 
    /// </summary>
    public bool IsClientRunning { get; private set; }

    /// <summary>
    /// 儲存資料事件：呼叫 WriteData 時觸發
    /// </summary>
    public Action<LabDataBase> WriteDataAction { get; set; }

    /// <summary>
    /// 上傳資料事件（非主執行緒事件，不可直接呼叫場景物件）
    /// string = "" OR float = -1 過濾
    /// </summary>
    public Action<SendInfo> SendDataAction { get; set; }    

    /// <summary>
    /// LabData Config
    /// </summary>
    private LabDataConfig _labDataConfig;
    

    // File ID
    /// <summary>
    /// <c>{BucketName}_{GameName}_{MotionDataID}</c>
    /// </summary>
    public string FileNamePre {get; private set;} = "";
    /// <summary>
    /// <c>{LabTools.DataPath}/ForStore/{UserID}</c>
    /// </summary>
    public string SaveDataPath {get; private set;} = "";
    /// <summary>
    /// <c>{LabTools.DataPath}/ForSend/{UserID}</c>
    /// </summary>
    public string SendDataPath {get; private set;} = "";

    /// <summary>
    /// 尚待寫入的資料數量
    /// </summary>
    public int DataCount => _dataQueue.Count;

    // Data Writer
    /// <summary>
    /// 資料寫入佇列 (thread-safe), {DataType, Appendix}
    /// </summary>
    private ConcurrentQueue<Tuple<LabDataBase, string>> _dataQueue;
    /// <summary>
    /// 監測 _dataQueue 執行緒
    /// </summary>
    private Thread _writeThread;
    /// <summary>
    /// _writeThread 執行緒信號, false 就是該關閉了
    /// </summary>
    private bool _writeThreadSignal;
    /// <summary>
    /// 各 "DataType+Appendix" 的 Writer
    /// </summary>
    private Dictionary<string, LabDataWriter> _dataWriterDic;
    
    // UI
    [SerializeField] private QuittingPanel quitPrefab;
    private QuittingPanel _quittingPanel;

    /* -------------------------------------------------------------------------- */

    void IManager.ManagerInit()
    {
        LabTools.Log("[LabDataManager] Start Init");
        
        // Get Config
        _labDataConfig = LabTools.GetConfig<LabDataConfig>(true);

        // Check/Request Permission       
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidHelper.CheckStoragePermission();        
#endif    

        // (PC) 等到 deinit 完成才准關閉
        Application.wantsToQuit += () => !IsInited;

        // Init
        GameData = new LabGameData();
        _dataWriterDic = new Dictionary<string, LabDataWriter>();
        _dataQueue = new ConcurrentQueue<Tuple<LabDataBase, string>>(); 
        _writeThreadSignal = true;
        _writeThread = new Thread(Queue2WriteThread);               
    }

    /// <summary>
    /// Dispose
    /// </summary>
    IEnumerator IManager.ManagerDispose()
    {
        LabTools.Log("[LabDataManager] Start Dispose");

        // Quit UI        
        if (_quittingPanel == null)
        {
            Transform container = null;
            try{ container = GameObject.FindGameObjectWithTag(LabTools.MAIN_CANVAS)?.transform; }catch{}
            container ??= FindObjectOfType<Canvas>()?.transform;
            container ??= transform.root;
            _quittingPanel = Instantiate<QuittingPanel>(quitPrefab, container);           
        }
        
        // Set unrunning 
        StopRunning();

        // wait for Writer finish
        _quittingPanel?.gameObject.SetActive(true);
        while (DataCount > 0)
        {
            LabTools.Log("[LabDataManager] Remain " + DataCount + " Data to be stored.");
            _quittingPanel?.UpdateInfo(DataCount.ToString());
            yield return new WaitForSeconds(1.0f);
        }
        _quittingPanel?.gameObject.SetActive(false);

        // Dispose all Writer
        foreach (var writer in _dataWriterDic.Values)
        {
            writer.Dispose();
        }

        // kill writer thread
        _writeThreadSignal = false;
        yield return new WaitForSeconds(0.2f);
        if(_writeThread.IsAlive)
            _writeThread.Abort();

        // Clear ForSend
        LabTools.DeleteAllEmptyDir(LabTools.FOR_SEND_DIR);

        // Clear Data
        GameData = null;
        WriteDataAction = null;
        SendDataAction = null;
        FileNamePre = "";
        _dataQueue = new ConcurrentQueue<Tuple<LabDataBase, string>>();
        _dataWriterDic = new Dictionary<string, LabDataWriter>();
        _writeThread = null;

        #if UNITY_IOS && !UNITY_EDITOR
        if (!string.IsNullOrEmpty(LabTools.DataPath) && LabTools.DataPath.StartsWith(iOSHelper.GetSendPath()))
        {
            iOSHelper.ReleaseSendPath(iOSHelper.GetSendPath());
        }
        #endif

        // Finish Dispose        
        IsInited = false;
        LabTools.Log("[LabDataManager] Disposed");
    }

    /// <summary>
    /// 存檔功能 啟動
    /// </summary>
    private void StartRunning()
    {
        if (IsClientRunning) 
            return;
        Debug.Log("[LabData] 啟動！開始接受新資料");
        IsClientRunning = true;
    }

    /// <summary>
    /// 停止存檔功能
    /// </summary>
    private void StopRunning()
    {
        if (!IsClientRunning) 
            return;
        Debug.Log("[LabData] 停止接受新資料");
        IsClientRunning = false;
    }

    #region Public Method

    /// <summary>
    /// 導入 UserID，初始化上傳功能
    /// </summary>
    /// <param name="userID"></param>
    public void LabDataInit(string userID, string motionIdOverride = "")
    {
        if (IsInited) 
            return;

        #region 驗證參數        
        if (string.IsNullOrEmpty(userID) )
        {
            throw new ArgumentNullException("userID", "UserID can not be empty.");
        }
        if(userID.Contains("_") || Path.GetInvalidFileNameChars().Any(userID.Contains))
        {
            throw new ArgumentException("UserID contains invalid characters.", "userID");
        }
        #endregion

        #region 初始化 GameID, DataPath
        // Check Permission
#if UNITY_ANDROID && !UNITY_EDITOR
        if(!AndroidHelper.CheckStoragePermission())
        {
            Debug.LogError("[LabDataManager] 權限不足，無法存取檔案，請確認是否有給予儲存權限！！");
            Application.Quit();
        }             
#endif        
        if(string.IsNullOrEmpty(_labDataConfig.GameID))
        {
            _labDataConfig.GameID = Application.productName.Replace("_", "");
            LabTools.Log($"已自動指定 GameID={_labDataConfig.GameID}");
        }
        // 初始化根目錄
        if (!string.IsNullOrEmpty(_labDataConfig.LocalSavePath) ) // 在 LabDataConfig 中已設定 LocalPath
        {
            LabTools.SetDataPath(_labDataConfig.LocalSavePath);
            if( !_labDataConfig.LocalSavePath.Contains(_labDataConfig.GameID))
                LabTools.SetDataPath(Path.Combine(LabTools.DataPath, _labDataConfig.GameID));
            LabTools.Log("已手動指定 DataPath="+LabTools.DataPath);
        }
        else
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            // Windows: {Documents}/LabData/{GameID}
            LabTools.SetDataPath(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                "LabData",
                _labDataConfig.GameID)
            );
#elif UNITY_ANDROID
            // Android: /storage/emulated/0/LabData/{GameID}
            LabTools.SetDataPath(Path.Combine("/storage/emulated/0/LabData/", _labDataConfig.GameID));
#elif UNITY_IOS
            // iOS Platform: {Application.persistentDataPath}/LabData/{GameID}
            string path = iOSHelper.GetSendPath();
            if(!string.IsNullOrEmpty(path))
            {
                LabTools.SetDataPath(Path.Combine(path, "LabData", _labDataConfig.GameID));
                LabTools.Log($"iOS AppGroup Path detected!  LabDataPath={LabTools.DataPath}");
            }
            else
            {
                Debug.LogError("[LabDataManager] iOS AppGroup Path not found, using default path.");
                LabTools.SetDataPath(Path.Combine(Application.persistentDataPath, "LabData", _labDataConfig.GameID));
            }
#else
            // Other Platform: {Application.persistentDataPath}/LabData/{GameID}
            LabTools.SetDataPath(Path.Combine(Application.persistentDataPath, "LabData", _labDataConfig.GameID));
            LabTools.Log($"Non-tested Platform detected!  LabDataPath={LabTools.DataPath}");
#endif
        }        
        #endregion

        #region File Name        
        FileNamePre = string.Join("_", 
            _labDataConfig.BucketID,
            _labDataConfig.GameID, 
            string.IsNullOrWhiteSpace(motionIdOverride) ? 
                DateTime.Now.ToString(_labDataConfig.LocalSaveDataTimeLayout) : 
                motionIdOverride //.PadLeft(3, '0')            
        );
        // if(!string.IsNullOrEmpty(_labDataConfig.GameMode))
        // {
        //     _fileName = string.Join("_", _fileName, _labDataConfig.GameMode);
        // }
        #endregion
        
        #region 初始化本地存檔 ForStore
        // Create folder ForStore
        SaveDataPath = Path.Combine(LabTools.DataPath, LabTools.FOR_STORE_DIR);
        LabTools.CreateSaveDataFolder(SaveDataPath);
        // Create folder for file
        SaveDataPath = Path.Combine(SaveDataPath, userID);
        SaveDataPath = LabTools.CreateSaveDataFolder(SaveDataPath);
        #endregion

        #region 初始化上傳功能 ForSend
        // Create folder ForSend
        SendDataPath = Path.Combine(LabTools.DataPath, LabTools.FOR_SEND_DIR);
        LabTools.CreateSaveDataFolder(SendDataPath);
        // Create folder for file
        SendDataPath = Path.Combine(SendDataPath, userID);
        SendDataPath = LabTools.CreateSaveDataFolder(SendDataPath);
        #endregion

        IsInited = true;
        StartRunning();        
        _writeThread.Start();
    }   

    /// <summary>
    /// 寫入資料
    /// </summary>
    /// <param name="data">資料</param>
    /// <param name="appendix">檔案後綴，如果有填會在檔案後面加上多一個_</param>
    public void WriteData(LabDataBase data, string appendix = "")
    {
        if(!IsClientRunning)
        {
            LabTools.LogError("LabData 未初始化或已停止接受資料");
            return;
        }

        var dataToWrite = new Tuple<LabDataBase, string>(data, appendix);
        _dataQueue.Enqueue(dataToWrite);
        WriteDataAction?.Invoke(data);
    }    

    /// <summary>
    /// 刪除特定 Motion ID 資料，回傳 false 如果沒有刪除任何資料
    /// <param name="motionId"></param>
    /// <param name="dir">刪除 ForSend、ForStore、或 Both（二者）（預設）</param>
    /// </summary>
    public bool DeleteData(string motionId, DataDir dir)
    {
        bool f = false;
        string path;

        switch(dir)
        {
            case DataDir.ForSend:  path = LabTools.FOR_SEND_DIR; break;
            case DataDir.ForStore: path = LabTools.FOR_STORE_DIR; break;
            // case DataDir.Both:     path = ""; break;
            default:               path = ""; break;
        }
        string[] files = LabTools.GetDataList(path);
        foreach(string target in files)
        {
            if( target.Contains(motionId))
            {
                File.Delete(target);
                f = true;
            }
        }

        return f;
    } 

    #endregion


    #region Save Data

    /// <summary>
    /// (由另外一個 thread 開啟) 持續監測 _dataQueue，將資料寫入
    /// </summary>
    private void Queue2WriteThread()
    {
        while (_writeThreadSignal)
        {
            // 休息一下
            Thread.Sleep(100);

            // check if inited
            if (!IsInited)
            {
                continue;
            }

            // Dequeue all data, attempt to write 
            List<Tuple<LabDataBase, string>> dataList = new List<Tuple<LabDataBase, string>>();
            while (_dataQueue.TryDequeue(out var data))
            {
                dataList.Add(data);
            }

            // Write data
            foreach (var dataTuple in dataList)
            {
                var data = dataTuple.Item1;
                var appendix = dataTuple.Item2;
                string key = $"{data.GetType().Name}_{appendix}";

                // if no writer exist for this, create one
                if(!_dataWriterDic.ContainsKey(key))
                {
                    _dataWriterDic[key] = NewWriter(data, SendDataPath, appendix);
                }

                // write data~
                _dataWriterDic[key].WriteData(data);
            }

            // TODO _dataWriterDic 數量檢查，避免過多 Writer 導致超過 File Handle 限制
            if(_dataWriterDic.Count > 300)
            {
                Debug.LogWarning("Too many writer exist! Please check if there is any writer not disposed.");
            }
        }
    }

    private LabDataWriter NewWriter(LabDataBase data, string dataPath, string fileNameAppendix)
    {
        // Create data file
        string fileName = $"{FileNamePre}_{data.GetType().Name}" + (string.IsNullOrEmpty(fileNameAppendix) ? "" : $"_{fileNameAppendix}") + ".json";
        string filePath = Path.Combine(dataPath, fileName);        
        LabDataWriter writer = new LabDataWriter(filePath);
        writer.InitData(data);
        return writer;
    }

    #endregion
}

public class LabDataWriter : IDisposable
{
    protected string _path;
    protected StreamWriter _stream;
    // protected bool _first = true;

    public LabDataWriter(string path)
    {
        _path = path;
        // LabTools.CreateFile(path);
        _stream = new StreamWriter(path, true, Encoding.UTF8);
    }
    public void InitData(LabDataBase data)
    {
        // _first = true;
    }
    public void WriteData(LabDataBase data)
    {    
        // if (!_first)
        // {
        //     File.WriteAllText(_path, ",");
        //     _first = false;
        // }

        string json = JsonConvert.SerializeObject(data, new JsonSerializerSettings{ReferenceLoopHandling = ReferenceLoopHandling.Ignore});        
        string content = json + Environment.NewLine;

        // Write using IO
        // File.AppendAllText(_path, content, Encoding.UTF8);

        // Write using StreamWriter
        _stream.Write(content);
        _stream.Flush();
    }

    public void Dispose()
    {
        // File.AppendAllText(_path, LabTools.JsonEnd + Environment.NewLine);
        _stream.Dispose();
    }
}
