using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using LabFrame2023;

public class LabFrame2023_Menu : MonoBehaviour
{

    [MenuItem("LabFrame2023/酷")]
    public static void UpdateAllConfigs()
    {
        EditorUtility.DisplayDialog("LabFrame2023", "酷", "酷");
    }
}