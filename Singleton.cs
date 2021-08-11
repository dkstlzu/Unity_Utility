using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType(typeof(T)) as T;

                    if (_instance == null)
                    {
                        Debug.LogError("There's no active " + typeof(T) + " in this scene");
                    }
                }

                return _instance;
            }
        }
    }
}