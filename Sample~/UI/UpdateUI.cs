using UnityEngine;
using LABFrame2022;
using System.Collections.Concurrent;
using System;

/// <summary>
/// For Thread to update UI 
/// </summary>
public class UpdateUI : Singleton<UpdateUI>
{
    private bool _active = false;

    private ConcurrentQueue<SendInfo> _updateInfo;
    public Action<SendInfo> InfoAction { get; set; }


    private void Start()
    {
        LABApplication.Instance.RestartAction += LABApp_Restart;
        LabDataManager.Instance.SendDataAction += QueueInfo;
        _updateInfo = new ConcurrentQueue<SendInfo>();
    }

    private void Update()
    {
        if (_active)
        {
            UpdateInfo();
        }
        
    }

    public void QueueInfo(SendInfo sendinfo)
    {
        _active = true;
        _updateInfo.Enqueue(sendinfo);

    }

    private void UpdateInfo()
    {
        if (_updateInfo.Count <= 0)
        {
            _active = false;
            return;
        }
        _updateInfo.TryDequeue(out var info);
        InfoAction?.Invoke(info);
    }

    private void LABApp_Restart()
    {
        LabDataManager.Instance.SendDataAction += QueueInfo;
#if UNITY_2021_3_OR_NEWER
        _updateInfo.Clear();
#endif
    }
}


