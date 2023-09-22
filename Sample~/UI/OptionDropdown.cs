using UnityEngine;
using UnityEngine.UI;

public class OptionDropdown : MonoBehaviour
{
    private Dropdown modes;

    void Start()
    {
        modes = gameObject.GetComponent<Dropdown>();

        // if no default option or no same option in dropdown , set to 0
        modes.value = 0;
        if (LabDataManager.Instance.Config.GameMode != "")
        {
            for (int i = 0; i < modes.options.Count; i++)
            {
                if (modes.options[i].text == LabDataManager.Instance.Config.GameMode)
                    modes.value = i;
            }
        }
        modes.onValueChanged.AddListener(dropdown_option);
    }

    private void dropdown_option(int index)
    {
        LabDataManager.Instance.Config.GameMode = modes.options[index].text;
        LabDataManager.Instance.SetConfig();
    }

}
