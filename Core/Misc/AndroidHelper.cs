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

    static AndroidHelper()
    {
        UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        CurrentActivity = UnityClass.GetStatic<AndroidJavaObject>("currentActivity");
    }
    
    /// <summary>
    /// 使用安裝包名開啟 APK
    /// </summary>
    /// <param name="packageName"></param>
    public static void OpenApk(string packageName)
    {

        using AndroidJavaObject packageManager = CurrentActivity.Call<AndroidJavaObject>("getPackageManager");
        using AndroidJavaObject launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);
        if (launchIntent != null)
        {
            // launchIntent.Call<AndroidJavaObject>("putExtra", "User_Info", user_Info); // put some more params
            CurrentActivity.Call("startActivity", launchIntent);
        }
    }
#endif
}