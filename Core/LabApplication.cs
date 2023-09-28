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
    private List<IManager> _managers = new List<IManager>();

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
        // Instantiate managers from Resources/IManagers
        var managerPrefabs = Resources.LoadAll<GameObject>("IManagers");
        foreach (var managerPrefab in managerPrefabs)
        {
            var managerGameObject = Instantiate(managerPrefab, transform);
            var manager = managerGameObject.GetComponent<IManager>();
            if (manager == null)
            {
                Debug.LogWarning($"Cannot find IManager in {managerPrefab.name}!");
                continue;
            }
            managerGameObject.name = manager.GetType().Name;
            _managers.Add(manager);
        }

        // _managers = FindObjectsOfType<MonoBehaviour>().OfType<IManager>().ToList();
        _managers.ForEach(p =>
        {
            p.ManagerInit();
        });
    }

    /// <summary>
    /// 退出遊戲
    /// </summary>
    private IEnumerator ApplicationDisposeAsync(DisposeOptions options = DisposeOptions.Quit)
    {
        // Dispose all managers
        for (int i = 0; i < _managers.Count; i++)
        {
            yield return StartCoroutine(_managers[i].ManagerDispose());        
        }
        _managers.Clear();

        // Dispose all child
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        // Do targeted action
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
        ApplicationDisposeAsync();
    }
    
    /// <summary>
    /// 框架重啟。
    /// 如果遊戲有「不離開遊戲而重新開始」需求時，請於重開時呼叫此 function。
    /// </summary>
    [Obsolete("Use AppRestartAsync() instead")]
    public void AppRestart()
    {
        AppRestartAsync();
    }

    /// <summary>
    /// 框架重啟。
    /// 如果遊戲有「不離開遊戲而重新開始」需求時，請於重開時呼叫此 function。
    /// </summary>
    public IEnumerator AppRestartAsync()
    {
        LabTools.Log("LabFrame Restarting... ");
        yield return ApplicationDisposeAsync(DisposeOptions.Restart);
    }
}







