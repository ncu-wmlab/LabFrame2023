using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New key", menuName = "LabFrame2022/SSHkey", order = 1)]
public class SSHKey : ScriptableObject
{
    [TextArea(15, 25)]
    public string key;
}
