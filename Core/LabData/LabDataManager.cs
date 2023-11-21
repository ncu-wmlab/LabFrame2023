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

public class LabDataManager : LabSingleton<LabDataManager>, IManager
{
    /// <summary>
    /// 遊戲運行資料，可自由呼叫
    /// </summary>
    public LabGameData GameData;

    /// <summary>
    /// 遊戲是否正在進行（PC）
    /// </summary>
    public bool IsClientRunning { get; private set; }
    /// <summary>
    /// 儲存資料事件
    /// </summary>
    public Action<LabDataBase> WriteDataAction { get; set; }
    /// <summary>
    /// 上傳資料事件（非主執行緒事件，不可直接呼叫場景物件）
    /// string = "" OR float = -1 過濾
    /// </summary>
    public Action<SendInfo> SendDataAction { get; set; }

    /// <summary>
    /// LabDataManager 是否已初始化 (是否呼叫過 LabDataInit)
    /// </summary>
    public bool IsInited { get; private set; } = false;

    // LabData Config
    private LabDataConfig _labDataConfig;
    

    // File ID
    private string _fileName = "";
    private string _saveDataPath = "";
    private string _sendDataPath = "";

    // Data Writer
    private ConcurrentQueue<LabDataBase> _dataQueue;
    private Thread _writeThread;
    private Dictionary<Type, LabDataWriter> _dataWriterDic;
    
    // UI
    [SerializeField] private QuittingPanel quitPrefab;
    private QuittingPanel _quittingPanel;

    void IManager.ManagerInit()
    {
        // Get Config
        _labDataConfig = LabTools.GetConfig<LabDataConfig>();

        // Check/Request Permission       
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidHelper.CheckStoragePermission();        
#endif    

        // (PC)
        Application.wantsToQuit += () => !IsClientRunning;

        GameData = new LabGameData();
        _dataWriterDic = new Dictionary<Type, LabDataWriter>();
        _dataQueue = new ConcurrentQueue<LabDataBase>(); 
        _writeThread = new Thread(Queue2Write);               
    }

    /// <summary>
    /// Dispose
    /// </summary>
    IEnumerator IManager.ManagerDispose()
    {
        // Quit UI        
        if (_quittingPanel == null)
        {
            Transform container = null;
            try{ container = GameObject.FindGameObjectWithTag(LabTools.MAIN_CANVAS)?.transform; }catch{}
            container ??= FindObjectOfType<Canvas>()?.transform;
            container ??= transform.root;
            _quittingPanel = Instantiate<QuittingPanel>(quitPrefab, container);           
        }
        
        // wait for Writer finish
        _quittingPanel?.gameObject.SetActive(true);
        while (_dataQueue.Count > 0 )
        {
            LabTools.Log("Remain " + _dataQueue.Count + " Data to be stored.");
            _quittingPanel?.UpdateInfo(_dataQueue.Count.ToString());
            yield return new WaitForSeconds(1.0f);
        }
        _quittingPanel?.gameObject.SetActive(false);

        // Dispose all Writer
        foreach (var item in _dataWriterDic)
        {
            item.Value.WriterDispose();
        }
        _writeThread.Abort();

        // Clear ForSend
        LabTools.DeleteAllEmptyDir(LabTools.FOR_SEND_DIR);

        // Clear Data
        GameData = null;
        WriteDataAction = null;
        SendDataAction = null;
        _fileName = "";
        _dataQueue = new ConcurrentQueue<LabDataBase>();
        _dataWriterDic = new Dictionary<Type, LabDataWriter>();        

        // Finish Dispose
        StopUpload();
        IsInited = false;
        LabTools.Log("LabUploadManager Dispose");
    }

    /// <summary>
    /// 存檔功能 啟動
    /// </summary>
    private void StartUpload()
    {
        if (IsClientRunning) 
            return;
        Debug.Log("[LabData] 啟動");
        IsClientRunning = true;
    }

    /// <summary>
    /// 停止存檔功能
    /// </summary>
    private void StopUpload()
    {
        if (!IsClientRunning) return;
        Debug.Log("[LabData] 停止");
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
            _labDataConfig.GameID = Application.productName;
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
#else
            // Other Platform: {Application.persistentDataPath}/LabData/{GameID}
            LabTools.SetDataPath(Path.Combine(Application.persistentDataPath, "LabData", _labDataConfig.GameID));
            LabTools.Log($"Non-tested Platform detected!  LabDataPath={LabTools.DataPath}");
#endif
        }        
        #endregion

        #region File Name        
        _fileName = string.Join("_", 
            _labDataConfig.BucketID,
            _labDataConfig.GameID, 
            string.IsNullOrWhiteSpace(motionIdOverride) ? 
                DateTime.Now.ToString(_labDataConfig.LocalSaveDataTimeLayout) : 
                motionIdOverride //.PadLeft(3, '0')            
        );
        if(!string.IsNullOrEmpty(_labDataConfig.GameMode))
        {
            _fileName = string.Join("_", _fileName, _labDataConfig.GameMode);
        }
        #endregion
        
        #region 初始化本地存檔 ForStore
        // Create folder ForStore
        _saveDataPath = Path.Combine( LabTools.DataPath, LabTools.FOR_STORE_DIR);
        LabTools.CreateSaveDataFolder(_saveDataPath);
        // Create folder for file
        _saveDataPath = Path.Combine( _saveDataPath, userID);
        _saveDataPath = LabTools.CreateSaveDataFolder(_saveDataPath);
        #endregion

        #region 初始化上傳功能 ForSend
        // Create folder ForSend
        _sendDataPath = Path.Combine( LabTools.DataPath, LabTools.FOR_SEND_DIR);
        LabTools.CreateSaveDataFolder(_sendDataPath);
        // Create folder for file
        _sendDataPath = Path.Combine(_sendDataPath, userID);
        _sendDataPath = LabTools.CreateSaveDataFolder(_sendDataPath);
        #endregion

        IsInited = true;

        StartUpload();        
        _writeThread.Start();
    }   

    /// <summary>
    /// 寫入資料
    /// </summary>
    public void WriteData(LabDataBase data)
    {
        _dataQueue.Enqueue(data);
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

    private void Queue2Write()
    {
        while (IsClientRunning)
        {
            var dataList = new List<LabDataBase>();
            while (_dataQueue.TryDequeue(out var resultData))
            {
                dataList.Add(resultData);
            }
            foreach (var d in dataList)
            {
                DoOnce(d);
            }
            Thread.Sleep(100);
        }
    }
    private void DoOnce(LabDataBase data)
    {
        if (!IsInited)
        {
            LabTools.LogError("LabData 尚未初始化");
            return;
        }
        DataWriterFunc(data);
    }
    private void DataWriterFunc(LabDataBase data)
    {
        var datatype = data.GetType();
        // string filePath = Config.SendToServer ? _sendDataPath : _saveDataPath;
        string filePath = _sendDataPath;  // FIXME　讓使用者選擇要不要上傳？
        // First time 
        if (!_dataWriterDic.ContainsKey(datatype))
        {
            // Add new Writer to Dic
            _dataWriterDic.Add(datatype, NewWriter(data, filePath));
        }

        _dataWriterDic[datatype].WriteData(data);
    }
    private LabDataWriter NewWriter( LabDataBase data, string dataPath)
    {
        // Create data file
        // SaveFileName = GameID_MotionID_DataType.json
        string path = Path.Combine(dataPath, $"{_fileName}_{data.GetType().Name}.json");
        LabTools.CreateData(path);

        LabDataWriter _LW = new LabDataWriter(path);
        _LW.InitData(data);
        return _LW;
    }

    #endregion

    #region Android 
    /*

    protected void OnApplicationPause(bool isPause)
    {
        LabTools.Log("Application Pause");
        if (isPause)
        {
            DataPause();
        }
        else
        {
            //DataResume();
        }
    }
    IEnumerator DataPause()
    {
        while (_dataQueue.Count > 0)
        {
            Debug.Log(($"Remain {0} Data to be stored", _dataQueue.Count));
            yield return new WaitForSeconds(1.0f);
        }

        foreach (var item in _dataWriterDic)
        {
            item.Value.SaveData();
        }
    }

    */
    #endregion

}

public class LabDataWriter
{
    private string _path;
    // private bool _first = true;
    public LabDataWriter(string path)
    {
        _path = path;
    }
    public void InitData(LabDataBase data)
    {
        // string jsonPrefix;
        // if (LabDataManager.Instance.Config.IsTest != true)
        // {
        //     jsonPrefix = LabTools.JsonPrefix.Replace("data", data.GetType().Name);
        // }
        // else
        // {
        //     jsonPrefix = LabTools.JsonPrefix.Replace("data", data.GetType().Name + LabTools.Test);
        // }
        // File.AppendAllText(_path, jsonPrefix);
    }
    public void WriteData(LabDataBase data)
    {    
        // if (!_first)
        // {
        //     File.WriteAllText(_path, ",");
        //     _first = false;
        // }
        
        File.AppendAllText(_path, JsonUtility.ToJson(data) + Environment.NewLine, Encoding.UTF8);
        
    }

    public void WriterDispose()
    {
        // FIXME 有時候根本沒叫到這邊
        // File.AppendAllText(_path, LabTools.JsonEnd + Environment.NewLine);
    }
}