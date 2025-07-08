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

        protected bool _isEnabled = false;

        /// <summary>
        /// 遊戲啟動時接 AIOT 的參數
        /// </summary>
        public void ManagerInit()                    
        {
            // 是否啟用 AIOT？
            _config = LabTools.GetConfig<AIOT_Config>(true);
            _isEnabled = _config.Enabled;
            if(!_isEnabled)
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
#elif UNITY_IOS
            paramJson = iOSHelper.GetLaunchParameters();
            Debug.Log("[AIOT] iOSHelper.GetLaunchParameters() 結果: " + paramJson);
            if(!string.IsNullOrEmpty(paramJson)) {
                _gameParams = JsonUtility.FromJson<AIOT_GameParams>(paramJson);
            }                    
#else
            Debug.LogError("[AIOT] Unsupported platform");
#endif
            // 拿到我們要的啟動參數
            if(string.IsNullOrEmpty(paramJson))
            {
                Debug.Log("[AIOT] 未透過 AIOT Platform 啟動：沒接收到啟動參數。");
                return;
            }            
            print("[AIOT] 啟動參數：" + paramJson);
            _gameParams = JsonUtility.FromJson<AIOT_GameParams>(paramJson);
        }

        /// <summary>
        /// 獲取遊戲的啟動參數
        /// 如果確實有啟動參數，系統會自動呼叫 LabDataManager.LabDataInit() 啟動資料採集模組
        /// </summary>
        /// <typeparam name="T">你的 CourseParams 類別</typeparam>
        public T GetCourseParams<T>()
        {
            if(_gameParams == null)
                return default; // null

            LabDataManager.Instance.LabDataInit(_gameParams.userId, _gameParams.MotiondataId);
            return JsonUtility.FromJson<T>(_gameParams.CourseParams);            
        }

        public IEnumerator ManagerDispose()
        {
            _config = null;
            _gameParams = null;
            // _isEnabled = false;
            yield break;
        }

        /// <summary>
        /// Callback sent to all game objects before the application is quit.
        /// </summary>
        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            
            if(!_isEnabled)
                return;

#if UNITY_ANDROID && !UNITY_EDITOR
            // 跳回 AIOT Platform App
            string packname = _config.AIOTPlatformPackageName;
            Debug.Log($"Now jumping back to AIOT Platform ({packname})");
            AndroidHelper.OpenApk(packname);
#elif UNITY_IOS && !UNITY_EDITOR
            // iOS 目前沒有跳回 AIOT Platform 的 API
            string packname = _config.AIOTPlatformPackageName;
            Debug.Log($"Let's go back to {packname} in iOS ");
            iOSHelper.OpenApplication("xrlab-"+packname);
#endif
        }
    }
}