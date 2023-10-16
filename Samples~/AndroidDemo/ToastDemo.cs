using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ToastDemo : MonoBehaviour
{
    [SerializeField] InputField _toastInput;
    [SerializeField] Button _toastButton;

    // Start is called before the first frame update
    void Start()
    {
        _toastButton.onClick.AddListener(MakeToast);
    }

    void MakeToast()
    {
        AndroidHelper.MakeToast(_toastInput.text);
    }
}