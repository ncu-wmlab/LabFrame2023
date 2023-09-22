

namespace LabFrame2023
{
    /// <summary>
    /// 遊戲中跨腳本、場景資料，可自由定義，Language 為示範
    /// </summary>
    public class LabGameData
    {
        public GameLanguage Language;

        public LabGameData(GameLanguage _language)
        {
            Language = _language;
        }

        public LabGameData()
        {
            Language = GameLanguage.Chinese;
        }
    }

    public enum GameLanguage
    {
        Chinese,
        English
    }
}