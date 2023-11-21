using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LabFrame2023;

public class CustomLabData : LabDataBase
{
    public int CoolInt = 0;
    public string CoolString = "COOL!";
    public List<float> CoolFloatList =new List<float>{4, 8, 7f, 6f, 3f, 0, 0, 0, 3.14159f};
    public CustomLabData_CustomObject CoolObject;
}

[System.Serializable]
public class CustomLabData_CustomObject
{
    public bool DemoObjectBool = false;
    public int[] DemoObjectIntArray = new int[]{1, 2, 3, 4, 5};
}