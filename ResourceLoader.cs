﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
#endif

namespace Utility
{
    public class ResourceLoader : EnumSettableMonoBehaviour 
#if UNITY_EDITOR
    , IPreprocessBuildWithReport 
#endif
    {
        [SerializeField] private bool ShowSettingsInEditor;
        [SerializeField] private bool ShowPathSceneInEditor;
        [SerializeField] private bool ShowDatasInEditor;
        [SerializeField] private bool ShowPreloadedResourcesInEditor;
        [SerializeField] private bool ShowSharedResourcesInEditor;
        [SerializeField] private bool ShowCurrentResourcesInEditor;
        [SerializeField] private int NamingInterval = 100;
        [SerializeField] private int SharingNamingRegion = 0;
        [SerializeField] private string SharingResourcesPath = string.Empty;
        [SerializeField] private string ResourcePathPrefix = string.Empty;
        [Serializable] public struct PathScene
        {
            public string Path;
            public string Scene;

            public PathScene (string path, string scene)
            {
                Path = path;
                Scene = scene;
            }
        }

        public List<UnityEngine.Object> PreloadedResourcesList = new List<UnityEngine.Object>();
        public Dictionary<Enum, UnityEngine.Object> SharedResourcesDict = new Dictionary<Enum, UnityEngine.Object>();
        public Dictionary<Enum, UnityEngine.Object> CurrentResourcesDict = new Dictionary<Enum, UnityEngine.Object>();

        public bool UsePathSceneSync;
        public List<PathScene> ResourcePathsForEachScene = new List<PathScene>();

        private int _currentRegion = 0;
        private int _namingStart
        {
            get {return _currentRegion * NamingInterval;}
        }
        private int _namingEnd
        {
            get {return _namingStart + NamingInterval - 1;}
        }

        void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoad;
            LoadShared();
        }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 30), "Test1"))
        {
            Load(1);
        }

        if (GUI.Button(new Rect(120, 10, 100, 30), "Test2"))
        {
        }
    }

        private List<string> ResourceSubDirectories = new List<string>();
#if UNITY_EDITOR
        public int callbackOrder
        {
            get;
        }
        public void OnPreprocessBuild(BuildReport report)
        {
            string ResourcesPath = Application.dataPath + "/Resources";
            string[] directories = Directory.GetDirectories(ResourcesPath, "*", SearchOption.AllDirectories);
            foreach (var item in directories)
            {
                string itemPath = item.Substring(ResourcesPath.Length + 1);
                ResourceSubDirectories.Add(itemPath);
            }
        }
#endif

        protected virtual void LoadShared()
        {
            int indexOfEnumStart, indexOfEnumEnd;

            EnumHelper.ClapIndexOfEnum(EnumValue.GetType(), _namingStart, _namingEnd, out indexOfEnumStart, out indexOfEnumEnd);

            int clapedLength = indexOfEnumEnd - indexOfEnumStart + 1;

            Enum[] namings = new Enum[clapedLength];
            Array.Copy(Enum.GetValues(EnumValue.GetType()), indexOfEnumStart, namings, 0, clapedLength);

            foreach(Enum e in namings)
            {   
                UnityEngine.Object source = Resources.Load<UnityEngine.Object>(Path.Combine(ResourcePathPrefix, SharingResourcesPath, e.ToString()));
                SharedResourcesDict.Add(e, source);
            }
        }

        public virtual void Load(int targetRegion)
        {
            _currentRegion = targetRegion;

            int indexOfEnumStart, indexOfEnumEnd;

            EnumHelper.ClapIndexOfEnum(EnumValue.GetType(), _namingStart, _namingEnd, out indexOfEnumStart, out indexOfEnumEnd);
            if (indexOfEnumStart < 0) 
            {
                print("Empty Enum Region");
                return;
            }

            int clapedLength = indexOfEnumEnd - indexOfEnumStart + 1;

            Enum[] namings = new Enum[clapedLength];
            Array.Copy(Enum.GetValues(EnumValue.GetType()), indexOfEnumStart, namings, 0, clapedLength);


            for (int i = 0; i < namings.Length; i++)
            {
// #if UNITY_EDITOR
                UnityEngine.Object source = ResourcesExtension.LoadSubDirectory<UnityEngine.Object>(ResourcePathPrefix, namings.GetValue(i).ToString());
// #elif UNITY_STANDALONE
//                 UnityEngine.Object source = ResourcesExtension.LoadSubDirectory<UnityEngine.Object>(ResourceSubDirectories, enumTemp.GetValue(i).ToString());
// #endif
                CurrentResourcesDict.Add(namings.GetValue(i) as Enum, source);
            }
        }

        public virtual UnityEngine.Object Load(string path)
        {
            string finalPath = Path.Combine(ResourcePathPrefix, path);

            UnityEngine.Object source = Resources.Load<UnityEngine.Object>(finalPath);
            Enum enumValue = Enum.Parse(EnumValue.GetType(), source.name) as Enum;
            CurrentResourcesDict.Add(enumValue, source);
            return source;
        }

        public virtual UnityEngine.Object[] LoadInFolder(string path)
        {
            string finalPath = Path.Combine(ResourcePathPrefix, path);

            UnityEngine.Object[] sources = Resources.LoadAll<UnityEngine.Object>(finalPath);
            foreach(UnityEngine.Object source in sources)
            {
                Enum enumValue = Enum.Parse(EnumValue.GetType(), source.name) as Enum;
                CurrentResourcesDict.Add(enumValue, source);
            }
            return sources;
        }

        protected virtual void ClearCurrent()
        {
            CurrentResourcesDict.Clear();
        }

        public T Get<T>(Enum sourceName) where T : UnityEngine.Object
        {
            UnityEngine.Object source;
            if (CurrentResourcesDict.TryGetValue(sourceName, out source)) {}
            else if (SharedResourcesDict.TryGetValue(sourceName, out source)) {}
            else if (source = PreloadedResourcesList.Find(x => x.name == sourceName.ToString())) {}
            else 
            {
                Debug.Log(sourceName + " is not loaded");
                return null;
            }

            if (source is T)
                return (T)source;
            else
                return null;
        }

        public static T SGet<T>(Enum sourceName) where T : UnityEngine.Object
        {
            ResourceLoader[] loaders = GameObject.FindObjectsOfType<ResourceLoader>();

            foreach (ResourceLoader loader in loaders)
            {
                if (loader.EnumValue.GetType() != sourceName.GetType()) continue;

                return loader.Get<T>(sourceName);
            }

            return null;
        }

        void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            if (!UsePathSceneSync) return;

            for (int i = 0; i < ResourcePathsForEachScene.Count; i++)
            {
                if (ResourcePathsForEachScene[i].Scene == scene.name)
                {
                    ClearCurrent();
                    LoadInFolder(ResourcePathsForEachScene[i].Path);
                    return;
                }
            }
        }        
    }
}