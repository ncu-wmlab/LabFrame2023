using LABFrame2022;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Text txt_Percent;
    public GameObject img_Progress;
    void Start()
    {
        if (txt_Percent != null && img_Progress != null)
            UpdateUI.Instance.InfoAction += UpdateProgress;
        else
            LabTools.Log("UI did not set up.");
    }

    void UpdateProgress(SendInfo sendInfo)
    {
        if( sendInfo.percent != -1f)
        {
            img_Progress.transform.localScale = new Vector3(sendInfo.percent, 1f, 1f);
            // to %
            sendInfo.percent *= 100f;
            txt_Percent.text = sendInfo.percent.ToString() + "%";
        }
    }
}
