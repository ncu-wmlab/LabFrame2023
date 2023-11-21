using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CustomLabDataDemo : MonoBehaviour
{
    [SerializeField] InputField _nameInput;
    [SerializeField] Button _labDataInitButton;
    [SerializeField] Button _writeDataButton;

    
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        _labDataInitButton.onClick.AddListener(DoInit);
        _writeDataButton.onClick.AddListener(DoWriteData);
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        _labDataInitButton.interactable = !LabDataManager.Instance.IsInited;
        _writeDataButton.interactable = LabDataManager.Instance.IsInited;
    }

    void DoInit()
    {
        if(string.IsNullOrEmpty(_nameInput.text))
        {
            LabPromptBox.Show("請輸入玩家名稱");
            return;
        }

        LabDataManager.Instance.LabDataInit(_nameInput.text);        
    }

    void DoWriteData()
    {
        // Generate a random CustomLabData
        var customDataObj = new CustomLabData_CustomObject{
            DemoObjectBool = Random.value > .5f,
            DemoObjectIntArray = new int[]{Random.Range(0, 100),1, 2, 3}
        };
        var customData = new CustomLabData{
            CoolInt = Random.Range(0, 100),
            CoolString = "COOL: "+Random.Range(0, 100),
            CoolFloatList = new List<float>{Random.value*100, 4, 8, 7f, 6f, 3f},
            CoolObject = customDataObj,
        };

        // Write to LabData
        LabDataManager.Instance.WriteData(customData);
        
        LabPromptBox.Show("Create OK");
    }
}