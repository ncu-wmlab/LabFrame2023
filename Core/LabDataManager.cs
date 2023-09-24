using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using LabFrame2023;

public class LabDataManager : Singleton<LabDataManager>, IManager
{
    /// <summary>
    /// LabData Config 設定
    /// </summary>
    public LabDataConfig Config { get { return _labDataConfig; } private set { _labDataConfig = value; } }
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

    // LabData Config
    private LabDataConfig _labDataConfig;
    // LabData Init
    private bool _isClientInit = false;
    // File ID
    private string _userID = "";
    private string _saveID = "";
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

        #region 初始化根目錄

        if (_labDataConfig.LocalSavePath != "") // 在 LabDataConfig 中已設定 LocalPath
        {
            LabTools.SetDataPath(_labDataConfig.LocalSavePath);
            if( !_labDataConfig.LocalSavePath.Contains(_labDataConfig.GameID))
                LabTools.SetDataPath(Path.Combine(LabTools.DataPath, _labDataConfig.GameID));
        }
        else
        {

#if UNITY_STANDALONE_WIN
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
            LabTools.LogWarning($"Non-tested Platform detected!  LabDataPath={LabTools.DataPath}");
#endif
            _labDataConfig.LocalSavePath = LabTools.DataPath;
            SetConfig();
        }

        #endregion

        // (PC) Not for mobile platform
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
        try
        {
            if (_quittingPanel == null)
            {
                Transform container = GameObject.FindGameObjectWithTag(LabTools.MAIN_CANVAS)?.transform;
                container = container ?? FindObjectOfType<Canvas>()?.transform;
                container = container ?? transform.root;
                _quittingPanel = Instantiate<QuittingPanel>(quitPrefab, container);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error when instantiating quitting panel: {e}");
        }
        

        // wait for Writer finish
        _quittingPanel?.gameObject.SetActive(true);
        while (_dataQueue.Count > 0 )
        {
            LabTools.Log("Remain " + _dataQueue.Count + " Data to be stored.");
            _quittingPanel.UpdateInfo(_dataQueue.Count.ToString());
            yield return new WaitForSeconds(1.0f);
        }
        _quittingPanel?.gameObject.SetActive(false);

        // Dispose all Writer
        foreach (var item in _dataWriterDic)
        {
            item.Value.WriterDispose();
        }
        // Clear ForSend
        LabTools.DeleteAllEmptyDir( LabTools.FOR_SEND_DIR);

        // Clear Data
        GameData = null;
        _userID = _saveID = "";
        WriteDataAction = null;
        SendDataAction = null;

        // Finish Dispose
        StopUpload();
        _isClientInit = false;
        LabTools.Log("LabUploadManager Dispose");
    }

    /// <summary>
    /// 存檔功能 啟動
    /// </summary>
    private void StartUpload()
    {
        if (IsClientRunning) return;
        Debug.Log("Start Upload");
        IsClientRunning = true;
    }

    /// <summary>
    /// 停止存檔功能
    /// </summary>
    private void StopUpload()
    {
        if (!IsClientRunning) return;
        Debug.Log("Stop Upload");
        IsClientRunning = false;
    }

    #region Public Method

    /// <summary>
    /// 回傳目前 ID，無則自動產生新 ID
    /// </summary>
    public string GetUserID()
    {
        // return ID
        if (_userID != "") return _userID;

        // Get new ID
        // _labDataConfig.SerialID++;
        // SetConfig();

        // FileID = GameID_LocID_UserID(_GameMode)
        _userID = string.Join("_", 
            _labDataConfig.GameID, 
            _labDataConfig.LocID,
            _labDataConfig.SerialID.ToString().PadLeft(3, '0')            
        );
        if(string.IsNullOrEmpty(_labDataConfig.GameMode))
        {
            _userID = string.Join("_", _userID, _labDataConfig.GameMode);
        }

        return _userID;
    }

    /// <summary>
    /// 導入 UserID，初始化上傳功能
    /// </summary>
    /// <param name="userID"></param>
    public void LabDataInit(string userID)
    {
        if (_isClientInit) return;

        // Check LabDataConfig
        // if( Config.ServerPath == "")
        // {
        //     LabTools.LogError("Config [ServerPath] can not be empty.");
        //     return;
        // }
        if( Config.GameID == "")
        {
            LabTools.LogError("Config [GameID] can not be empty.");            
            Application.Quit(404);
            return;
        }

        // Set MotionID
        if ( string.IsNullOrEmpty(userID) )
        {
            LabTools.Log("[UserID] is empty. Data would only save to local.");
            // Config.SendToServer = false;

            _userID = GetUserID();
            _saveID = string.Join("_", DateTime.Now.ToString(_labDataConfig.LocalSaveDataTimeLayout), _userID);
        }
        else
        {
            _userID = userID;
            _saveID = _userID;
        }           

        #region 初始化本地存檔 ForStore
        // Create folder ForStore
        _saveDataPath = Path.Combine( LabTools.DataPath, LabTools.FOR_STORE_DIR);
        LabTools.CreateSaveDataFolder(_saveDataPath);
        // Create folder for file
        _saveDataPath = Path.Combine( _saveDataPath, _saveID);
        _saveDataPath = LabTools.CreateSaveDataFolder(_saveDataPath);
        #endregion

        #region 初始化上傳功能 ForSend
        // Create folder ForSend
        _sendDataPath = Path.Combine( LabTools.DataPath, LabTools.FOR_SEND_DIR);
        LabTools.CreateSaveDataFolder(_sendDataPath);
        // Create folder for file
        _sendDataPath = Path.Combine(_sendDataPath, _saveID);
        _sendDataPath = LabTools.CreateSaveDataFolder(_sendDataPath);
        #endregion

        StartUpload();
        _isClientInit = true;
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

    /// <summary>
    /// 修改 LabData Config
    /// </summary>
    public void SetConfig()
    {
        LabTools.WriteConfig(_labDataConfig, true);
    }

    /// <summary>
    /// 還原 LabData Config 
    /// </summary>
    public void ResetConfig()
    {
        _labDataConfig = LabTools.GetConfig<LabDataConfig>(true);
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
        }
    }
    private void DoOnce(LabDataBase data)
    {
        if (!_isClientInit)
        {
            LabTools.Log("LabData 尚未初始化");
            return;
        }

        DataWriterFunc(data);
    }
    private void DataWriterFunc(LabDataBase data)
    {
        var datatype = data.GetType();
        // string filePath = Config.SendToServer ? _sendDataPath : _saveDataPath;
        string filePath = _saveDataPath;
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
        string dataName = string.Join("_", 
            _labDataConfig.GameID ,
            _userID,
            data.GetType().Name + ".json"
        );
        string path = Path.Combine(dataPath, dataName);
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