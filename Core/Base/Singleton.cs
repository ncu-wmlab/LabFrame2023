using System;
using UnityEngine;

namespace LabFrame2023
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T m_Instance;

        public static T Instance
        {
            get
            {
                if (object.ReferenceEquals(Singleton<T>.m_Instance, null))
                {
                    Singleton<T>.m_Instance = (UnityEngine.Object.FindObjectOfType(typeof(T)) as T);
                    if (object.ReferenceEquals(Singleton<T>.m_Instance, null))
                    {
                        Debug.LogWarning("cant find a gameobject of instance " + typeof(T) + "!");
                    }
                    else
                    {
                        Singleton<T>.m_Instance.OnAwake();
                    }
                }
                return Singleton<T>.m_Instance;
            }
        }

        public static bool IsInstanceValid
        {
            get
            {
                return !object.ReferenceEquals(Singleton<T>.m_Instance, null);
            }
        }

        private void Awake()
        {
            if (object.ReferenceEquals(Singleton<T>.m_Instance, null))
            {
                Singleton<T>.m_Instance = (this as T);
                Singleton<T>.m_Instance.OnAwake();
            }
        }

        protected virtual void OnAwake()
        {
        }

        protected virtual void OnApplicationQuit()
        {
            Singleton<T>.m_Instance = (T)((object)null);
        }

        protected virtual void DoOnDestroy()
        {
        }

        private void OnDestroy()
        {
            this.DoOnDestroy();
            if (object.ReferenceEquals(Singleton<T>.m_Instance, this))
            {
                Singleton<T>.m_Instance = (T)((object)null);
            }
        }
    }

}
