using System.Collections;
using System.Collections.Generic;
using LabFrame2023;
using UnityEngine;
using UnityEngine.UI;

public class LabPromptBoxDemo : MonoBehaviour
{
    [SerializeField] InputField _input;

    public void CreatePrompt()
    {
        LabPromptBox.Show(_input.text, () => {
            LabPromptBox.Show("You clicked Confirm");
        });
    }

    public void CreatePromptWithCancel()
    {
        LabPromptBox.Show(_input.text, () => {
            LabPromptBox.Show("You clicked Confirm");
        }, () => {
            LabPromptBox.Show("You clicked Cancel");
        });
    }
}