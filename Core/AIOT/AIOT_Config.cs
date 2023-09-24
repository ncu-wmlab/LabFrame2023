namespace LabFrame2023.AIOT
{
    public class AIOT_Config
    {
        /// <summary>
        /// 是否啟用 AIOT
        /// </summary>
        public bool Enabled = false;
        /// <summary>
        /// AIOT 伺服器 IP
        /// </summary>
        public string ServerUrl = "http://ncuaiot-dev.ap-northeast-1.elasticbeanstalk.com/";
        /// <summary>
        /// AIOT Platform 的安裝包名稱 (Android)
        /// </summary>
        public string AIOTPlatformPackageName = "com.NCUVRLAB.NCUAIOT";
    }
}