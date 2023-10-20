using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LabFrame2023
{
    public class LabTools
    {
        // Frame Default path
        public const string CONFIG_DIR = "Config";
        public const string FOR_STORE_DIR = "ForStore";
        public const string FOR_SEND_DIR = "ForSend";
        // public const string JsonPrefix = "{ \"data\" : [ ";
        // public const string Test = "!Test";
        // public const string JsonEnd = " ] }";
        public const string MAIN_CANVAS = "MainCanvas";

        /// <summary>
        /// 設定檔存放路徑
        /// </summary>
        public static string ConfigPath { get; private set; }
        /// <summary>
        /// 資料存放路徑
        /// 依平台不同 (由 LabDataManager 設定)
        /// </summary>
        public static string DataPath { get; private set; }
        /// <summary>
        /// 可開關是否 Log
        /// </summary>
        public static bool IsLog { get; private set; } = true;
        /// <summary>
        /// 框架輸出 Log
        /// </summary>
        public static Action<string> LogAction;


        #region Static Constructor

        static LabTools()
        {
#if UNITY_EDITOR
            // Assets/Resources/Config  
            // 後續遊戲 build 出來後會以此為範本去複製
            ConfigPath = Path.Combine(Application.dataPath, "Resources", CONFIG_DIR);
#elif UNITY_STANDALONE_WIN
            // {遊戲資料夾}/Config
            ConfigPath = Path.Combine(Application.dataPath, CONFIG_DIR);
#elif UNITY_ANDROID
            // {Application.persistentDataPath}/Config
            ConfigPath = Path.Combine(Application.persistentDataPath, CONFIG_DIR);
#else
            ConfigPath = Path.Combine(Application.persistentDataPath, CONFIG_DIR);
            Debug.Log("Untested platform, use default path: " + ConfigPath");
#endif
        }

        #endregion


        // public static T GetData<T>(LabDataBase data) where T : LabDataBase
        // {
        //     return data is T @base ? @base : null;
        // }



        #region Data Method

        /// <summary>
        /// 設定新檔案位置（DataPath）
        /// </summary>
        /// <typeparam name="T">資料夾路徑。如果該路徑沒有資料夾，則會先建立一個</typeparam>
        public static void SetDataPath(string path)
        {
            Log("[LabTools] Set Data Path: " + path);
            DataPath = path;
            if (!Directory.Exists(DataPath))
            {
                Directory.CreateDirectory(path);
            }            
        }

        /// <summary>
        /// 創建檔案資料夾，isNew 可以同名不同時間點創建
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="isNew"></param>
        public static string CreateSaveDataFolder(string folderName, bool isNew = false)
        {
            if (Directory.Exists(folderName))
            {
                if (isNew)
                {
                    var tempPath = folderName + "_" + DateTime.Now.Millisecond.ToString();
                    Directory.CreateDirectory(tempPath);
                    return tempPath;
                }
                // Log("Folder Has Existed!");
                return folderName;
            }
            else
            {
                Directory.CreateDirectory(folderName);
                Log("Success Create: " + folderName);
                return folderName;
            }
        }
        
        /// <summary>
        /// 創建檔案
        /// </summary>
        /// <param name="path"></param>
        public static void CreateData(string path)
        {
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();

                Log("Success Create: " + path);
            }
            else
            {
                Log("File already exsist! path: " + path);
            }

        }

        /// <summary>
        /// 獲取路徑位置的資料夾
        /// </summary>
        /// <param name="path"></param>
        public static DirectoryInfo GetFolder(string path)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Log("File not exsist! path: " + path);
                return null;
            }
            else
            {
                return new DirectoryInfo(Path.GetDirectoryName(path));
            }

        }

        /// <summary>
        /// 獲取本地端某路徑下的所有 json 資料（包括子資料夾）
        /// <param name="filePath"></param>
        /// </summary>
        public static string[] GetDataList(string filePath = "")
        {
            string path;
            string[] files;

            if (DataPath.EndsWith("LabData"))   // 全部遊戲
            {
                path = DataPath;
                files = Directory.GetFileSystemEntries(path, "*.json", SearchOption.AllDirectories);

                if( filePath != "") // 有指定
                {
                    List<string> list_files = new List<string>();
                    foreach (string target in files)
                    {
                        if ( target.Contains(filePath))
                        {
                            list_files.Add(target);
                        }
                    }

                    files = list_files.ToArray();
                }
                
                
            }
            else // 特定遊戲
            {
                path = Path.Combine(DataPath, filePath);
                if (!Directory.Exists(path))
                {
                    Log(path + " is not exist!");
                    return null;
                }
                files = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);
            }
            

            return files;
        }

        /// <summary>
        /// 刪除本地端遊戲某路徑下的所有空資料夾
        /// </summary>
        public static void DeleteAllEmptyDir( string filePath)
        {
            string path;
            if (DataPath.EndsWith("LabData"))   // 全部遊戲
            {
                path = DataPath;
                string[] gameDirs = Directory.GetDirectories(path);
                foreach(string gameDir in gameDirs)  // 進入各個遊戲檢查
                {
                    path = Path.Combine(gameDir, filePath);
                    string[] targetDir = Directory.GetDirectories(path);
                    foreach( var target in targetDir)
                    {
                        if (!Directory.EnumerateFileSystemEntries(target).Any())
                        {
                            Directory.Delete(target);
                        }
                    }
                }
            }
            else // 特定遊戲
            {
                path = Path.Combine(DataPath, filePath);
                if (!Directory.Exists(path))
                {
                    Log(path + " is not exsist!");
                    return;
                }

                string[] targetDir = Directory.GetDirectories(path);
                foreach (var dir in targetDir)
                {
                    if (!Directory.EnumerateFileSystemEntries(dir).Any())
                    {
                        Directory.Delete(dir);
                    }

                }
            }

            
        }

        /// <summary>
        /// 刪除本地端某路徑下的資料（DataPath 底下）
        /// <param name="filePath"></param>
        /// </summary>
        public static void DeleteData(string filePath)
        {
            string path = Path.Combine(DataPath, filePath);
            if (!File.Exists(path))
            {
                Log(path + " is not exsist!");
                return;
            }

            File.Delete(path);
        }

        #endregion

        #region Config method
        /// <summary>
        /// 取得對應 Config 檔設定資料
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="onlyUseTemplate">設定 true 表示"執行檔"只使用範例模板</param>
        /// <returns></returns>
        public static T GetConfig<T>(bool onlyUseTemplate = false) where T : class, new()
        {
            var path = ConfigPath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // path
            path = Path.Combine( path, typeof(T).Name + ".json");

            if (File.Exists(path) && (!Application.isEditor && !onlyUseTemplate))
            {        
#if UNITY_EDITOR
                // 編輯器內直接重建新的，以確保欄位完整性
                T t = new T();   
                try 
                {
                    t = JsonUtility.FromJson<T>(File.ReadAllText(path));
                } 
                catch (Exception e)
                {
                    Debug.LogWarning("讀取現有設定檔失敗，已建立一個全新的設定檔。請記得前往設定內容。\n" + e.Message);
                }
                File.WriteAllText(path, JsonUtility.ToJson(t, true));
                return t;
#else
                return JsonUtility.FromJson<T>(File.ReadAllText(path));
#endif                        
            }            
            else // 找不到 config 檔
            {
                T t;
                // 從 Resources 找個範例檔案
                var file = Resources.Load<TextAsset>("Config/"+typeof(T).Name);
                if(file == null)
                {
                    Log($"未找到{typeof(T).Name} 的設定檔，並且也沒在 Resources/{typeof(T).Name} 找到範例，已建立一個全新的設定檔。請記得前往設定內容。");
                    t = new T();
                }
                else
                {
                    Log($"未找到{typeof(T).Name} 的設定檔，而在 Resources 找到範例，已依此建立設定檔。");
                    t = JsonUtility.FromJson<T>(file.text);
                }
                File.WriteAllText(path, JsonUtility.ToJson(t, true));
                return t;
            }            
        }

        #endregion

        /// <summary>
        /// 統一生成 Log 
        /// </summary>
        public static void Log( string log)
        {
            if(IsLog)
            {
                Debug.Log(log);
                LogAction?.Invoke(log);
            }
        }

        /// <summary>
        /// 統一生成 LogError 
        /// </summary>
        public static void LogError( string log)
        {
            if (IsLog)
            {
                Debug.LogError(log);
                LogAction?.Invoke(log);
            }
        }


    }


    /// <summary>
    /// 傳送資料事件用資訊
    /// </summary>
    [Serializable]
    public class SendInfo
    {
        public string info = "";
        public float percent = -1f;

        public SendInfo(string _info)
        {
            info = _info;
        }

        public SendInfo(float _percent)
        {
            percent = _percent;
        }

        public SendInfo(string _info, float _percent)
        {
            info = _info;
            percent = _percent;
        }
    }

    /// <summary>
    /// 資料存檔地點
    /// </summary>
    public enum DataDir
    {
        ForSend,
        ForStore,
        Both
    }
}

