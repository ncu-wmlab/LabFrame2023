using System;
using UnityEngine;

namespace LabFrame2023
{
    public abstract class LabSingleton<T> : MonoBehaviour where T : LabSingleton<T>
    {
        protected static LabApplication m_labApplicationInstance;
        private static T m_Instance;

        public static T Instance
        {
            get
            {
                if (object.ReferenceEquals(LabSingleton<T>.m_Instance, null))
                {
                    // find from scene                    
                    LabSingleton<T>.m_Instance = (UnityEngine.Object.FindObjectOfType(typeof(T)) as T);
                    if (object.ReferenceEquals(LabSingleton<T>.m_Instance, null))
                    {
                        // load LabFrame from Resources
                        if(object.ReferenceEquals(LabSingleton<T>.m_labApplicationInstance, null))
                        {
                            Debug.Log("Init LabFrame from Resources");
                            var g = Instantiate(Resources.Load<GameObject>("LabFrame"));
                            m_Instance = g.GetComponentInChildren<T>();
                        }
                        if(object.ReferenceEquals(LabSingleton<T>.m_Instance, null))
                            Debug.LogError("Cannot find a gameobject of instance " + typeof(T).Name + "!");
                    }
                }
                return LabSingleton<T>.m_Instance;
            }
        }

        public static bool IsInstanceValid
        {
            get
            {
                return !object.ReferenceEquals(LabSingleton<T>.m_Instance, null);
            }
        }

        private void Awake()
        {
            // Current No Instance
            if (object.ReferenceEquals(LabSingleton<T>.m_Instance, null))
            {
                LabSingleton<T>.m_Instance = (this as T);
            }
            else
            {
                if(m_Instance)
                {
                    Debug.Log($"An instance of {typeof(T)} already exists. Destroying new one.");
                    Destroy(gameObject);
                }
                else
                {
                    Debug.LogError($"An instance of {typeof(T)} already exists. However it is null. Please ensure that the instance shall not be destroyed.");
                    LabSingleton<T>.m_Instance = (this as T);
                }
            }
        }

        protected virtual void OnApplicationQuit()
        {
            LabSingleton<T>.m_Instance = (T)((object)null);
        }

        protected virtual void DoOnDestroy()
        {
        }

        private void OnDestroy()
        {
            this.DoOnDestroy();
            if (object.ReferenceEquals(LabSingleton<T>.m_Instance, this))
            {
                LabSingleton<T>.m_Instance = (T)((object)null);
            }
        }
    }

}
