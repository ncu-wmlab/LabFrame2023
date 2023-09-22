using UnityEngine;
using UnityEngine.UI;

public class TestToggle : MonoBehaviour
{
    private Toggle IsTest;
    void Start()
    {

        IsTest = gameObject.GetComponent<Toggle>();
        IsTest.isOn = LabDataManager.Instance.Config.IsTest;
        IsTest.onValueChanged.AddListener(toggle_Upload);
    }

    private void toggle_Upload(bool isOn)
    {
        LabDataManager.Instance.Config.IsTest = isOn;
        LabDataManager.Instance.SetConfig();
    }
}