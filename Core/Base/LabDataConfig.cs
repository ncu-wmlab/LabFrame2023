using System;
using UnityEngine;

namespace LabFrame2023 
{
    [Serializable]
    public class LabDataConfig
    {
        // public bool SendToServer;
        public bool IsTest;
        /// <summary>
        /// (Optional) 資料庫 IP 位址
        /// 如果沒有用到上傳服務，則不需要設定
        /// </summary>
        // public string ServerPath;            
        /// <summary>
        /// 遊戲 ID ，用以建立資料庫根資料夾
        /// </summary>
        public string GameID;
        /// <summary>
        /// (Optional) 地點 ID
        /// </summary>
        public string LocID;        
        /// <summary>
        /// 資料序列號，一般從 0 開始，用以定義 UserID
        /// </summary>
        public int SerialID;
        /// <summary>
        /// （Optional）遊戲模式
        /// </summary>
        public string GameMode;
        /// <summary>
        /// (Optional) 本地存檔位置
        /// 可為空，空值代表預設位置 (依平台不同)
        /// 注意：修改此值可能導致無法上傳資料
        /// </summary>
        public string LocalSavePathOverride;
        /// <summary>
        /// 儲存檔案時，資料夾的 Timestamp 命名格式
        /// </summary>
        public string LocalSaveDataTimeLayout;    
        

        public LabDataConfig()
        {
            // SendToServer = true;
            IsTest = false;
            // ServerPath = "";            

            GameID = "";
            LocID = "";
            SerialID = 0;
            GameMode = "";

            LocalSavePathOverride = "";
            LocalSaveDataTimeLayout = "yyyyMMddHH";
        }
    }
}