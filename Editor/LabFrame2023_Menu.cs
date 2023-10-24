using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using LabFrame2023;

public class LabFrame2023_Menu : MonoBehaviour
{
    protected const string NAME = "LabFrame2023/";

    [MenuItem(NAME+"選擇開發平台 Choose Development Platform")]
    static void ChoosePlatform()
    {
        EditorWindow.GetWindow(typeof(LabFrameChoosePlatformWindow));
    }

    [MenuItem(NAME+"酷")]
    static void UpdateAllConfigs()
    {
        EditorUtility.DisplayDialog("LabFrame2023", "酷", "酷");
    }
}