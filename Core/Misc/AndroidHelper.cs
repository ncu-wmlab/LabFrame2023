using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AndroidHelper
{
#if UNITY_ANDROID
    /// <summary>
    /// com.unity3d.player.UnityPlayer 
    /// </summary>
    public static readonly AndroidJavaClass UnityClass;
    /// <summary>
    /// currentActivity Object
    /// </summary>
    public static readonly AndroidJavaObject CurrentActivity;
    /// <summary>
    /// UnityContext Object
    /// </summary>
    public static readonly AndroidJavaObject UnityContext;
    /// <summary>
    /// com.xrlab.labframe_plugin.Main
    /// </summary>
    protected static readonly AndroidJavaClass _pluginClass;
#endif

    static AndroidHelper()
    {
        if(Application.platform != RuntimePlatform.Android)
            return;
#if UNITY_EDITOR
        Debug.LogWarning("[AndroidHelper] Android plugin is not available in Editor.");
#elif UNITY_ANDROID
        UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        CurrentActivity = UnityClass.GetStatic<AndroidJavaObject>("currentActivity");        
        UnityContext = CurrentActivity.Call<AndroidJavaObject>("getApplicationContext");
        _pluginClass = new AndroidJavaClass("com.xrlab.labframe_plugin.Main");
#endif
    }


    /// <summary>
    /// 檢查/請求儲存相關的權限
    /// </summary>
    public static bool CheckStoragePermission()
    {
#if UNITY_ANDROID
        return _pluginClass.CallStatic<bool>("RequestStoragePermission", Application.identifier);
#else        
        return true;
#endif
    }
    
    /// <summary>
    /// 使用安裝包名開啟 APK
    /// </summary>
    /// <param name="packageName"></param>
    public static void OpenApk(string packageName, bool quitOnLaunch = true)
    {
        // Android 11 require android.permission.QUERY_ALL_PACKAGES permission
        // using AndroidJavaObject packageManager = CurrentActivity.Call<AndroidJavaObject>("getPackageManager");
        // using AndroidJavaObject launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);
        // if (launchIntent != null)
        // {
        //     // launchIntent.Call<AndroidJavaObject>("putExtra", "User_Info", user_Info); // put some more params
        //     CurrentActivity.Call("startActivity", launchIntent);
        // }
#if UNITY_ANDROID
        _pluginClass.CallStatic("OpenApk", packageName);
        if(quitOnLaunch)
            Application.Quit();
#endif
    }

    /// <summary>
    /// 製作一個 toast (Android 螢幕下方的一條提示訊息)
    /// </summary>
    /// <param name="msg">訊息</param>
    public static void MakeToast(string msg)
    {
#if UNITY_ANDROID
        _pluginClass.CallStatic("MakeToast", msg);
#endif
    }
}