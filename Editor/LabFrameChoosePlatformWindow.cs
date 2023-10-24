using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LabFrameChoosePlatformWindow : EditorWindow
{
    GUIContent _windowInformation = new GUIContent("選擇開發平台 - LabFrame 2023", "酷");

    bool _init = false;
    bool _usePico = false;
    bool _useVive = false;


    void OnGUI()
    {
        if(!_init)
        {
            titleContent = _windowInformation;
            var def = DefineSymbolsUtil.GetDefines();
            _usePico = def.Contains("USE_PICO");
            _useVive = def.Contains("USE_VIVE_ANDROID");
            _init = true;
        }
        BeginWindows();

        GUILayout.Space(20);
        GUILayout.Label("*選擇開發平台*");
        GUILayout.Label("勾選以下選項以開啟該平台框架功能\n(需要依賴於該平台的 SDK)");

        GUILayout.Space(20);
        _usePico = GUILayout.Toggle(_usePico, "PICO");
        _useVive = GUILayout.Toggle(_useVive, "HTC VIVE (Android 一體機)");

        GUILayout.Space(20);
        if(GUILayout.Button("確定"))
        {
            if(_usePico)
                DefineSymbolsUtil.Add("USE_PICO");
            else
                DefineSymbolsUtil.Remove("USE_PICO");
            if(_useVive)
                DefineSymbolsUtil.Add("USE_VIVE_ANDROID");
            else
                DefineSymbolsUtil.Remove("USE_VIVE_ANDROID");
        }

        EndWindows();
    }

}