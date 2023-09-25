using System;

namespace LabFrame2023
{
    [Serializable]
    public class LabDataBase
    {
        public string Timestamp;
        
        public LabDataBase()
        {
            Timestamp = DateTimeOffset.Now.ToString("o");
        }
    }
}
