using System;
using UnityEngine;

namespace dkstlzu.Utility
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
                        if (_instance == null) throw new NullReferenceException();
                    } catch (NullReferenceException e)
                    {
                        Debug.LogWarning(e.ToString() + "\nThere's no active " + typeof(T) + " in this scene");
                    }
                }

                return _instance;
            }
        }

        public static T GetOrCreateInstance
        {
            get
            {
                if (_instance == null)
                {
                    try
                    {
                        _instance = FindObjectOfType(typeof(T)) as T;
                        if (_instance == null) throw new NullReferenceException();
                    } catch (NullReferenceException)
                    {
                        Debug.LogWarning("There's no active " + typeof(T) + " in this scene\nBut made default one");
                        GameObject go = new GameObject(typeof(T).ToString() + "Singleton");
                        _instance = go.AddComponent<T>();
                    }
                }

                return _instance;
            }
        }
    }
}