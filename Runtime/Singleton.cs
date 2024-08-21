using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace dkstlzu.Utility
{
    public static class Singleton
    {
        private static Dictionary<Type, object> _dict = new Dictionary<Type, object>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RuntimeInit()
        {
            _dict.Clear();
        }
        
        public static T Get<T>(bool withOutNullNotify = false) where T : class
        {
            Type targetType = typeof(T);
            
            if (_dict.TryGetValue(targetType, out object result))
            {
                return result as T;
            }
            else
            {
                if (!withOutNullNotify)
                {
                    Printer.Print($"{typeof(T)} singleton is not registered yet");
                }
                return null;
            }
        }

        public static object Get(Type type)
        {
            if (_dict.TryGetValue(type, out object result))
            {
                return result;
            }
            else
            {
                Printer.Print($"{type.FullName} singleton is not registered yet");
                return null;
            }
        }

        public static T GetOrCreate<T>() where T : class, new()
        {
            Type targetType = typeof(T);

            if (!_dict.ContainsKey(targetType))
            {
                _dict.Add(targetType, new T());
            }
            
            return _dict[targetType] as T;
        }
        
        public static void RegisterSingleton(object obj)
        {
            Type targetType = obj.GetType();
            
            Assert.IsFalse(_dict.ContainsKey(targetType), "Singleton Instance is already constructed");
            
            _dict.Add(targetType, obj);
        }
    }
    
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static GameObject singletonGameObject;

        private static T _instance;

        public static T GetOrNull
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindObjectOfType<T>();
                }
                
                return _instance;
            }
        }

        public static T GetOrCreate()
        {
            if (!_instance)
            {
                _instance = FindObjectOfType<T>();
            }

            if (_instance)
            {
                return _instance;
            }
            
            if (!singletonGameObject)
            {
                singletonGameObject = new GameObject("Singleton GameObject");
            }
            
            if (!_instance)
            {
                _instance = singletonGameObject.AddComponent<T>();
            }

            return _instance;
        }

        public static T GetOrCreateDontDestroyOnLoad()
        {
            if (!_instance)
            {
                var go = new GameObject($"{typeof(T)} Singleton");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<T>();
            }

            return _instance;
        }
    }
}