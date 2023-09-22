using LABFrame2022;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Uploader : MonoBehaviour
{
    [Header("UI")]
    public Button btn_upload;
    public Button btn_reload;
    public Button btn_reset;
    public Button btn_quit;
    public TextPanel uploadInfo;
    public InputField input_DataDir;

    void Start()
    {
        btn_upload.onClick.AddListener(btn_Upload);
        btn_reload.onClick.AddListener(btn_Reload);
        btn_reset.onClick.AddListener(btn_Reset);
        btn_quit.onClick.AddListener(btn_Quit);
        Init();
    }
    // 初始化
    private void Init()
    {
        UpdateUI.Instance.InfoAction += LabDataManager_Info;
        LabTools.LogAction += LabTools_Info;

        input_DataDir.text = LabTools.DataPath;
    }
    // 一般 Log 訂閱
    private void LabTools_Info(string info)
    {
        if (uploadInfo != null)
        {
            uploadInfo.UpdateInfo(info);
        }
    }
    // 上傳 Log 訂閱
    private void LabDataManager_Info( SendInfo sendInfo)
    {
        if (uploadInfo != null)
        {
            uploadInfo.UpdateInfo(sendInfo.info);
        }
    }
    

    private void btn_Upload()
    {
        uploadInfo.UpdateInfo("Status：Uploading ...");
        LabDataManager.Instance.UploadData();

    }

    private void btn_Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void btn_Reload()
    {
        UpdateUI.Instance.InfoAction -= LabDataManager_Info;
        LabTools.LogAction -= LabTools_Info;

        LABApplication.Instance.AppRestart();
        Init();
        uploadInfo.ClearInfo();
        uploadInfo.UpdateInfo("Status：Reloaded");
    }

    private void btn_Reset()
    {
        UpdateUI.Instance.InfoAction -= LabDataManager_Info;
        LabTools.LogAction -= LabTools_Info;

        LabDataManager.Instance.ResetConfig();
        Init();
        uploadInfo.ClearInfo();
        uploadInfo.UpdateInfo("Status：Reset");
    }
}
