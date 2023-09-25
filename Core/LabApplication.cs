using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LabFrame2023;

public class LabApplication : LabSingleton<LabApplication>
{
    public enum DisposeOptions
    {
        Restart,
        Quit
    }

    /// <summary>
    /// Manager List
    /// </summary>
    private List<IManager> _managers;

    private void Awake()
    {
        if(IsInstanceValid) // Already has an instance, destroy this one
        {
            Destroy(gameObject);
            return;
        }
        m_labApplicationInstance = this;
        DontDestroyOnLoad(this);
        ApplicationInit();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void ApplicationInit()
    {
        _managers = FindObjectsOfType<MonoBehaviour>().OfType<IManager>().ToList();
        _managers.ForEach(p =>
        {
            p.ManagerInit();
        });
    }

    /// <summary>
    /// 退出遊戲
    /// </summary>
    private void ApplicationDispose(DisposeOptions options = DisposeOptions.Quit)
    {
        StartCoroutine(ApplicationDisposeAsync(options));
    }
    private IEnumerator ApplicationDisposeAsync(DisposeOptions options = DisposeOptions.Quit)
    {
        if (_managers.Count == 0)
        {
            yield break;
        }

        for (int i = 0; i < _managers.Count; i++)
        {
            yield return StartCoroutine(_managers[i].ManagerDispose());
        }
        _managers.Clear();

        switch (options)
        {
            case DisposeOptions.Restart:
                ApplicationInit();
                yield return null;
                break;
            case DisposeOptions.Quit:
                Application.Quit();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        yield return null;
    }

    // Ensure all IManagers has disposed before quit.
    // NOTE: 有些平台不會叫此 function！
    // e.g. android 滑掉 app 、iOS 要設定 exitOnSuspend、WEBGL 完全沒辦法、或是 OS 強制殺掉 app
    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        ApplicationDispose();
    }
    
    /// <summary>
    /// 框架重啟。
    /// 如果遊戲有「不離開遊戲而重新開始」需求時，請於重開時呼叫此 function。
    /// </summary>
    public void AppRestart()
    {
        LabTools.Log("LabFrame Restarting... ");
        ApplicationDispose(DisposeOptions.Restart);
    }
}







