using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public class iOSHelper
{
    #if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern bool _iOS_RequestStoragePermission();
    
    [DllImport("__Internal")]
    private static extern bool _iOS_OpenApplication(string urlScheme);
    
    [DllImport("__Internal")]
    private static extern void _iOS_ShowNotification(string message, float duration);
    
    [DllImport("__Internal")]
    private static extern string _iOS_GetLaunchParameters();
    
    [DllImport("__Internal")]
    private static extern void _iOS_FreeString(string str);

    [DllImport("__Internal")]
    private static extern string _GetAppGroupForsendPathChecked();

    [DllImport("__Internal")]
    private static extern void _ReleaseAppGroupPath(string path);
    #endif
    
    /// <summary>
    /// 檢查/請求儲存權限 (照片庫)
    /// 與 AndroidHelper.CheckStoragePermission 對應
    /// </summary>
    public static bool CheckStoragePermission()
    {
        #if UNITY_IOS && !UNITY_EDITOR
        return _iOS_RequestStoragePermission();
        #else
        Debug.Log("[iOSHelper] Storage permission request simulated in non-iOS platform");
        return true;
        #endif
    }
    
    /// <summary>
    /// 開啟外部應用程式
    /// 與 AndroidHelper.OpenApk 對應
    /// </summary>
    /// <param name="urlScheme">應用程式的 URL Scheme，例如 "myapp://"</param>
    /// <returns>是否成功發起開啟請求</returns>
    public static bool OpenApplication(string urlScheme, bool quitOnSuccess = true)
    {
        #if UNITY_IOS && !UNITY_EDITOR
        bool success = _iOS_OpenApplication(urlScheme);
        if(success && quitOnSuccess)
            Application.Quit();
        return success;
        #else
        Debug.Log($"[iOSHelper] Open application request simulated: {urlScheme}");
        return false;
        #endif
    }
    
    /// <summary>
    /// 顯示通知消息 (類似 Android Toast)
    /// 與 AndroidHelper.MakeToast 對應
    /// </summary>
    /// <param name="message">要顯示的消息</param>
    /// <param name="duration">顯示時長 (秒)</param>
    public static void ShowNotification(string message, float duration = 2.0f)
    {
        #if UNITY_IOS && !UNITY_EDITOR
        _iOS_ShowNotification(message, duration);
        #else
        Debug.Log($"[iOSHelper] Notification simulated: {message}, Duration: {duration}s");
        #endif
    }
    
    /// <summary>
    /// 獲取應用啟動時的參數 (從 URL Scheme 或 Universal Link)
    /// 這個 Android 沒有對應功能，是 iOS 特有的
    /// </summary>
    /// <returns>JSON 格式的啟動參數</returns>
    public static string GetLaunchParameters()
    {
        #if UNITY_IOS && !UNITY_EDITOR
        string parameters = _iOS_GetLaunchParameters();
        return parameters;
        #else
        Debug.Log("[iOSHelper] GetLaunchParameters called in non-iOS platform, returning empty string");
        return "";
        #endif
    }


    /// <summary>
    /// get the path for send file to other app
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetSendPath()
    {
        string path = null;
#if UNITY_IOS && !UNITY_EDITOR
        path = _GetAppGroupForsendPathChecked();
        if (!string.IsNullOrEmpty(path))
            return path;
        return null;
#else
        return path;
#endif
    }

    /// <summary>
    /// 釋放/清理 App Group 共享目錄中的檔案
    /// </summary>
    /// <param name="path">由 GetSendPath() 獲取的路徑</param>
    public static void ReleaseSendPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("[iOSHelper] Cannot release null or empty path");
            return;
        }

        #if UNITY_IOS && !UNITY_EDITOR
        _ReleaseAppGroupPath(path);
        #else
        Debug.Log($"[iOSHelper] ReleaseSendPath simulated for path: {path}");
        #endif
    }
}