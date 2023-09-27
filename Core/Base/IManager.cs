using System.Collections;
using UnityEngine;

public interface IManager
{
    /// <summary>
    /// Init
    /// 注意有可能會重新呼叫 (當遊戲重新開始時)
    /// </summary>
    void ManagerInit();

    /// <summary>
    /// Dispose the IManager.
    /// 清理 IManager，e.g. 把變數初始化。
    /// 後續有可能是要重開框架或是關閉遊戲
    /// </summary>
    /// <returns></returns>
    IEnumerator ManagerDispose();

    /// <summary>
    /// 繼承自 MonoBehaviour 的 gameObject
    /// </summary>
    /// <value></value>
    // GameObject gameObject {get;}
}