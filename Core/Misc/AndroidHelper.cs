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
    /// com.xrlab.labframe_plugin.Main
    /// </summary>
    protected static readonly AndroidJavaClass _pluginClass;


    static AndroidHelper()
    {
        UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        CurrentActivity = UnityClass.GetStatic<AndroidJavaObject>("currentActivity");        
        _pluginClass = new AndroidJavaClass("com.xrlab.labframe_plugin.Main");
    }


    /// <summary>
    /// 檢查/請求儲存相關的權限
    /// </summary>
    public static bool CheckStoragePermission()
    {
        return _pluginClass.CallStatic<bool>("RequestStoragePermission", CurrentActivity, Application.identifier);
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
        
        _pluginClass.CallStatic("OpenApk", CurrentActivity, packageName);

        if(quitOnLaunch)
            Application.Quit();
    }

    /// <summary>
    /// 製作一個 toast (Android 螢幕下方的一條提示訊息)
    /// </summary>
    /// <param name="msg">訊息</param>
    public static void MakeToast(string msg)
    {
        _pluginClass.CallStatic("MakeToast", CurrentActivity, msg);
    }
#endif
}