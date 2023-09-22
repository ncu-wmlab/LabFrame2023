using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextPanel : MonoBehaviour
{
    public Text txt_uploadInfo;
    public ScrollRect scroll_uploadScroll;

    public void UpdateInfo(string txt)
    {
        if (txt_uploadInfo != null && txt != "")
            txt_uploadInfo.text += Environment.NewLine + txt;
            StartCoroutine(ScrollText());
    }

    public void ClearInfo()
    {
        if (txt_uploadInfo != null)
            txt_uploadInfo.text = "";
        StartCoroutine(ScrollText());
    }

    IEnumerator ScrollText()
    {
        // Wait for end of frame AND force update all canvases before setting to bottom.
        if (scroll_uploadScroll == null)
            yield break;
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        scroll_uploadScroll.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }
}
