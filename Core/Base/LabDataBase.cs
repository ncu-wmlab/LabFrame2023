using System;

namespace LabFrame2023
{
    [Serializable]
    public class LabDataBase
    {
        public string Timestamp;
        public string DataID;
        
        public LabDataBase()
        {
            Timestamp = DateTimeOffset.Now.ToString("o");
            DataID = LabDataManager.Instance.GetUserID();
        }
    }
}
