using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

namespace dkstlzu.Utility
{
    public static class Singleton
    {
        private static GameObject _singletonGameObject;
        public static GameObject SingletonGameObject
        {
            get
            {
                if (!_singletonGameObject)
                {
                    _singletonGameObject = new GameObject("Singleton GameObject");
                }

                return _singletonGameObject;
            }
        }

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
            
            if (!_instance)
            {
                _instance = Singleton.SingletonGameObject.AddComponent<T>();
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

    public class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        private const string RESOURCES_FOLDER_NAME = "Resources";
        public const string FOLDER_NAME = "Scriptables";
        public static string ResourcesPath => Path.Combine(FOLDER_NAME, typeof(T).Name);
        
        private static T _instance;

        public static T GetOrNull
        {
            get
            {
                if (!_instance)
                {
                    _instance = Resources.Load<T>(ResourcesPath);
                }
                
                return _instance;
            }
        }

        public static T GetOrCreate()
        {
            _instance = GetOrNull;

            if (!_instance)
            {
                _instance = CreateInstance<T>();
#if UNITY_EDITOR
                string folderPath = Path.Combine(Application.dataPath, RESOURCES_FOLDER_NAME, FOLDER_NAME);
                
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    UnityEditor.AssetDatabase.Refresh();
                }

                string path = Path.Combine("Assets", RESOURCES_FOLDER_NAME, $"{ResourcesPath}.asset");
                UnityEditor.AssetDatabase.CreateAsset(_instance, path);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
#endif
            }

            return _instance;
        }
    }
}