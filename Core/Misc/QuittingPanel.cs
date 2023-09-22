using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LabFrame2023
{
    public class QuittingPanel : MonoBehaviour
    {
        public Text txt_Tile;
        public Text txt_SaveInfo;
        public Text txt_Info;
        private void OnEnable()
        {
            switch (LabDataManager.Instance.GameData.Language)
            {
                case GameLanguage.Chinese:
                    txt_Tile.text = "資料儲存中 ...";
                    txt_Info.text = "請勿關閉本程式，以免造成資料損失\r\n本程序完成後會自動關閉。";
                    break;
                case GameLanguage.English:
                    txt_Tile.text = "Saving Data ...";
                    txt_Info.text = "To avoid data loss, please do NOT close the program\r\nThis program will automatically close after completion.";
                    break;
                default:
                    txt_Tile.text = "資料儲存中 ...";
                    txt_Info.text = "請勿關閉本程式，以免造成資料損失\r\n本程序完成後會自動關閉。";
                    break;
            }
        }

        public void UpdateInfo(string info)
        {
            switch (LabDataManager.Instance.GameData.Language)
            {
                case GameLanguage.Chinese:
                    txt_SaveInfo.text = "還有 " + info + " 筆資料儲存中";
                    break;
                case GameLanguage.English:
                    txt_SaveInfo.text = "Remain " + info + " Data to be stored.";
                    break;
                default:
                    txt_SaveInfo.text = "還有 " + info + " 筆資料儲存中";
                    break;
            }

        }
    }

}