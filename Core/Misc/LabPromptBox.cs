using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Linq;

public class LabPromptBox : MonoBehaviour
{
    [SerializeField] Image _bg;
    [SerializeField] Text _content;
    [SerializeField] Button _confirmButton;
    [SerializeField] Button _cancelButton;

    public void Init(string content, Action onConfirm = null, Action onCancel = null)
    {
        _content.text = content;

        _confirmButton.onClick.RemoveAllListeners();
        _confirmButton.onClick.AddListener(() => {
            onConfirm?.Invoke();
            Hide();
        });

        if(onCancel == null)
            _cancelButton.gameObject.SetActive(false);
        _cancelButton.onClick.RemoveAllListeners();
        _cancelButton.onClick.AddListener(() => {
            onCancel?.Invoke();
            Hide();
        });

        bool anyCanvasIsNotWorldMode = FindObjectsOfType<Canvas>().Any(c => c.renderMode != RenderMode.WorldSpace);
        if(anyCanvasIsNotWorldMode)
        {
            gameObject.GetComponentInChildren<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            Debug.Log("[LabPromptBox] 偵測到有 Canvas 的 render mode 使用 Overlay 或 Camera：將此 LabPromptBox 的 render mode 設為 Overlay");
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        Destroy(gameObject, 1);
    }

    /* -------------------------------------------------------------------------- */

    IEnumerator Start()
    {
        Camera cam = Camera.main;
        while(true)
        {
            transform.position = cam.transform.position + cam.transform.forward * .301f;  // 放在攝影機前面距離 0.3 的位置
            transform.LookAt(cam.transform);
            transform.Rotate(0, 180, 0);  // 旋轉
            yield return new WaitForSecondsRealtime(.5f);
        }
    }

    /* -------------------------------------------------------------------------- */

    /// <summary>
    /// 顯示提示框
    /// </summary>
    /// <param name="content">要顯示的訊息</param>
    /// <param name="onConfirm">按下確認後要做的事 (可為 null)</param>
    /// <param name="onCancel">按下取消後要做的事 (可為 null，null 表示不顯示取消按鈕)</param>
    public static void Show(string content, Action onConfirm = null, Action onCancel = null)
    {
        var prefab = Resources.Load<LabPromptBox>("LabPromptBox");
        if(prefab == null)
            prefab = Resources.Load<LabPromptBox>("DefaultPrefabs/LabPromptBox");
        var instance = Instantiate(prefab);
        instance.Init(content, onConfirm, onCancel);
    }
}