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

    private ApplicationConfig _applicationConfig;
    public ApplicationConfig Config { get { return _applicationConfig; } private set { _applicationConfig = value; } }
    public Action RestartAction;


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
        _applicationConfig = LabTools.GetConfig<ApplicationConfig>();

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
        StartCoroutine(ApplicationDisposeEnumerator(options));
    }
    private IEnumerator ApplicationDisposeEnumerator(DisposeOptions options = DisposeOptions.Quit)
    {
        if (_managers.Count <= 0)
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
                RestartAction?.Invoke();
                break;
            case DisposeOptions.Quit:
                Application.Quit();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        yield return null;
    }

    // Ensure the game dispose before quit
    // NOTE: Will NOT work if android tap out
    protected void OnApplicationQuit()
    {
        ApplicationDispose();
        // base.OnApplicationQuit();
    }
    
    /// <summary>
    /// 框架重啟
    /// </summary>
    public void AppRestart()
    {
        LabTools.Log("Restarting ... ");
        ApplicationDispose(DisposeOptions.Restart);
    }
}







