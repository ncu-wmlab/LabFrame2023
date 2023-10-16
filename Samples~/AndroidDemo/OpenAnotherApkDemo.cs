using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenAnotherApkDemo : MonoBehaviour
{
    [SerializeField] InputField _packageNameInput;
    [SerializeField] Button _launchButton;

    // Start is called before the first frame update
    void Start()
    {
        _launchButton.onClick.AddListener(OpenApk);
    }

    void OpenApk()
    {
        AndroidHelper.OpenApk(_packageNameInput.text);
    }

}
 