using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace LabFrame2023.AIOT
{
    public class AIOT_Manager : Singleton<AIOT_Manager>, IManager
    {
        protected AIOT_GameParams _gameParams = null;

        public void ManagerInit()                    
        {
            //
            var config = LabTools.GetConfig<AIOT_Config>();
            if(!config.Enabled)
                return;

            // 從 AIOT_Platform 抓資料
            string paramJson = "";

#if UNITY_STANDALONE_WIN
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1) // 忽略第一個參數，因為它是執行檔案本身的路徑
            {
                paramJson = args[1];
            }

#elif UNITY_ANDROID
            using AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using AndroidJavaObject Activity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using AndroidJavaObject intent = Activity.Call<AndroidJavaObject>("getIntent");
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
                Debug.LogWarning("[AIOT] 未透過 AIOT Platform 啟動：沒接到啟動參數。");
                return;
            }            
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
            // TODO 跳回 AIOT Platform
            yield return 0;
        }
    }
}