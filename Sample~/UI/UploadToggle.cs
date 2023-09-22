using UnityEngine;
using UnityEngine.UI;

public class UploadToggle : MonoBehaviour
{
    private Toggle IsUpload;

    void Start()
    {
        IsUpload = gameObject.GetComponent<Toggle>();
        IsUpload.isOn = LabDataManager.Instance.Config.SendToServer;
        IsUpload.onValueChanged.AddListener(toggle_Upload);
    }

    private void toggle_Upload(bool isOn)
    {
        LabDataManager.Instance.Config.SendToServer = isOn;
        LabDataManager.Instance.SetConfig();
    }
}
