using System;
using UnityEngine;

namespace LabFrame2023
{
    [Serializable]
    public class ApplicationConfig
    {
        public bool ExternalOpen;

        public ApplicationConfig()
        {
            ExternalOpen = false;
        }
    }
}
