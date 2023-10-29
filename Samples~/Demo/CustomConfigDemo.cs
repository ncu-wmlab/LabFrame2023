using System.Collections;
using System.Collections.Generic;
using LabFrame2023;
using UnityEngine;
using UnityEngine.UI;

public class CustomConfigDemo : MonoBehaviour
{
    [SerializeField] Text _text;

    CustomConfig _config;

    
    void Start()
    {
        _config = LabTools.GetConfig<CustomConfig>();        
    }

    void Update()
    {
        _text.text = $"MyCoolInt={_config.MyCoolInt}\nMyCoolString={_config.MyCoolString}\nMyCoolFloatList={string.Join(",", _config.MyCoolFloatList)}";
    }

    public void PlusPlus()
    {
        _config.MyCoolInt++;

        // write data
        LabTools.WriteConfig(_config);
    }

    public void ResetConfig()
    {
        _config = LabTools.ResetConfig<CustomConfig>();
    }
}