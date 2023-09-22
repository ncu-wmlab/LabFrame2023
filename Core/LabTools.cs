using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LabFrame2023
{
    public class LabTools
    {
        // Frame Define
        public const string configDir = "Config";
        public const string saveDir = "ForStore";
        public const string sendDir = "ForSend";
        public const string JsonPrefix = "{ \"data\" : [ ";
        public const string Test = "!Test";
        public const string JsonEnd = " ] }";
        public const string MainCanvas = "MainCanvas";

        /// <summary>
        /// 設定檔存放路徑
        /// </summary>
        public static string ConfigPath = Application.persistentDataPath;
        /// <summary>
        /// 資料存放路徑
        /// </summary>
        public static string DataPath = "LabData";
        /// <summary>
        /// 可開關是否 Log
        /// </summary>
        public static bool IsLog = true;
        /// <summary>
        /// 框架輸出 Log
        /// </summary>
        public static Action<string> LogAction;

        public static T GetData<T>(LabDataBase data) where T : LabDataBase
        {
            return data is T @base ? @base : null;
        }



        #region Data Method

        /// <summary>
        /// 設定新檔案位置（DataPath）
        /// </summary>
        /// <typeparam name="T">資料夾路徑。如果該路徑沒有資料夾，則會先建立一個</typeparam>
        public static void SetDataPath(string path)
        {
            DataPath = path;
            if (!Directory.Exists(DataPath))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// 創建檔案資料夾（以資料類型命名）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void CreateDataFolder<T>(string filePath = configDir) where T : LabDataBase
        {
            var path = Path.Combine( DataPath, filePath, typeof(T).Name);
            if (!Directory.Exists(path))
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

                Log("Folder Has Existed!");
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
        /// <param name="isNew">設定 true 可以刪掉設定檔</param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static T GetConfig<T>(bool isNew = false, string filePath = configDir) where T : class, new()
        {
            var path = Path.Combine( ConfigPath, filePath);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine( path, typeof(T).Name + ".json");
            if (isNew && File.Exists(path))
            {
                File.Delete(path);
            }
            if (!File.Exists(path))
            {
                string json;
                var file = Resources.Load<TextAsset>(typeof(T).Name);
                if( file != null )
                {
                    json = file.text;
                }
                else
                {
                    Log("No " + typeof(T).Name + " example config, create default one.");
                    json = JsonUtility.ToJson(new T());
                }

                FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(json);
                sw.Close();
            }

            StreamReader sr = new StreamReader(path);
            var data = JsonUtility.FromJson<T>(sr.ReadToEnd());
            sr.Close();
            return data;
        }

        /// <summary>
        /// 檢查本地是否儲存過 Config 檔
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="isNew"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool CheckConfig<T>(string filePath = configDir) where T : class
        {
            var path = Path.Combine( ConfigPath, filePath, typeof(T).Name + ".json");
            if (File.Exists(path))
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// 寫入對應 Config 設定資料
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="dataName"></param>
        /// <param name="isOverWrite"></param>
        /// <returns></returns>
        public static void WriteConfig<T>(T t, bool isOverWrite = false, string filePath = configDir) where T : class, new()
        {
            // 檢查資料夾
            var path = Path.Combine(ConfigPath, filePath);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine( path, typeof(T).Name + ".json");
            if (!File.Exists(path))
            {
                var json = JsonUtility.ToJson(t);
                var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(json);
                sw.Close();
            }
            else if (File.Exists(path) && isOverWrite)
            {
                var json = JsonUtility.ToJson(t);
                var fs = new FileStream(path, FileMode.Truncate, FileAccess.ReadWrite);
                fs.Close();
                fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(json);
                sw.Close();
            }
            else
            {
                Log("如果需要覆蓋資料，請在 Config 中設置 isOverWrite = true");
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

