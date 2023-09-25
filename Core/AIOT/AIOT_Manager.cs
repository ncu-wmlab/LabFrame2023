using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace LabFrame2023.AIOT
{
    public class AIOT_Manager : LabSingleton<AIOT_Manager>, IManager
    {
        protected AIOT_Config _config = null; 
        protected AIOT_GameParams _gameParams = null;

        /// <summary>
        /// 遊戲啟動時接 AIOT 的參數
        /// </summary>
        public void ManagerInit()                    
        {
            // 是否啟用 AIOT？
            _config = LabTools.GetConfig<AIOT_Config>();
            if(!_config.Enabled)
                return;

            // 從 AIOT_Platform 抓資料
            string paramJson = "";

#if UNITY_EDITOR
            // TODO 從編輯器模擬啟動參數
#elif UNITY_STANDALONE_WIN
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1) // 忽略第一個參數，因為它是執行檔案本身的路徑
            {
                paramJson = args[1];
            }
#elif UNITY_ANDROID            
            using AndroidJavaObject intent = AndroidHelper.CurrentActivity.Call<AndroidJavaObject>("getIntent");
            if (intent.Call<bool>("hasExtra", "User_Info"))
            {
                using AndroidJavaObject extras = intent.Call<AndroidJavaObject>("getExtras");
                paramJson = extras.Call<string>("getString", "User_Info");
            }
#else
            Debug.LogError("[AIOT] Unsupported platform");                    
#endif
            // 拿到我們要的啟動參數
            if(string.IsNullOrEmpty(paramJson))
            {
                Debug.LogWarning("[AIOT] 未透過 AIOT Platform 啟動：沒接收到啟動參數。");
                return;
            }            
            print("[AIOT] 啟動參數：" + paramJson);
            _gameParams = JsonUtility.FromJson<AIOT_GameParams>(paramJson);
        }

        /// <summary>
        /// 獲取遊戲的啟動參數
        /// </summary>
        /// <typeparam name="T">你的 CourseParams 類別</typeparam>
        public T GetCourseParams<T>()
        {
            if(_gameParams == null)
                return default; // null
            return JsonUtility.FromJson<T>(_gameParams.CourseParams);            
        }

        public IEnumerator ManagerDispose()
        {
            // 跳回 AIOT Platform
#if UNITY_ANDROID
            AndroidHelper.OpenApk(_config.AIOTPlatformPackageName);
#endif
            yield return 0;
        }
    }
}