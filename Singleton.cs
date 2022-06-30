using System;
using UnityEngine;

namespace Utility
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T _instance;

        public static T instance
        {
            get
            {
                if (_instance == null)
                {
                    try
                    {
                        _instance = FindObjectOfType(typeof(T)) as T;
                    } catch (NullReferenceException e)
                    {
                        
                        Debug.LogError(e.ToString() + "\nThere's no active " + typeof(T) + " in this scene");
                    }
                }

                return _instance;
            }
        }
    }
}