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
        /// (Optional) 遊戲 ID ，用以建立資料庫根資料夾
        /// 填空值表示使用遊戲名稱 (PlayerSettings.ProductName)
        /// </summary>
        public string GameID;
        /// <summary>
        /// (Optional) AIOT 儲存貯體名稱
        /// </summary>
        public string BucketID;        
        /// <summary>
        /// （Optional）遊戲模式
        /// </summary>
        public string GameMode;
        /// <summary>
        /// (Optional) 本地存檔位置
        /// 可為空，空值代表預設位置 (依平台不同)
        /// 注意：修改此值可能導致無法上傳資料
        /// </summary>
        public string LocalSavePath;
        /// <summary>
        /// 儲存檔案時，資料的 Timestamp 命名格式
        /// </summary>
        public string LocalSaveDataTimeLayout;    
        

        public LabDataConfig()
        {
            // SendToServer = true;
            IsTest = false;
            // ServerPath = "";            

            GameID = "";
            BucketID = "";
            GameMode = "";

            LocalSavePath = "";
            LocalSaveDataTimeLayout = "yyyyMMddHHmmss";
        }
    }
}