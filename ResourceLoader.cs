using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utility
{
    public class ResourceLoader : EnumSettableMonoBehaviour
    {
        [SerializeField] private bool ShowSettingsInEditor;
        [SerializeField] private bool ShowPathSceneInEditor;
        [SerializeField] private bool ShowDatasInEditor;
        [SerializeField] private bool ShowPreloadedResourcesInEditor;
        [SerializeField] private bool ShowSharedResourcesInEditor;
        [SerializeField] private bool ShowCurrentResourcesInEditor;
        [SerializeField] private int NamingInterval = 100;
        [SerializeField] private int SharingNamingRegion = 0;
        [SerializeField] private string SharingSoundsPath = string.Empty;
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
        }

        void Start()
        {
            LoadShared();
        }

        protected virtual UnityEngine.Object[] LoadShared()
        {
            int indexOfEnumStart, indexOfEnumEnd;

            EnumHelper.ClapIndexOfEnum(EnumValue.GetType(), _namingStart, _namingEnd, out indexOfEnumStart, out indexOfEnumEnd);

            int clapedLength = indexOfEnumEnd - indexOfEnumStart + 1;

            Enum[] namings = new Enum[clapedLength];
            Array enumTemp = Enum.GetValues(EnumValue.GetType());
            Array.Copy(enumTemp, indexOfEnumStart, namings, 0, clapedLength);


            UnityEngine.Object[] sources = new UnityEngine.Object[clapedLength];

            for (int i = 0; i < enumTemp.Length; i++)
            {
                UnityEngine.Object source = ResourcesExtension.LoadSubDirectory<UnityEngine.Object>(ResourcePathPrefix, enumTemp.GetValue(i).ToString()) as UnityEngine.Object;
                SharedResourcesDict.Add(enumTemp.GetValue(i) as Enum, source);
                sources[i] = source;
            }

            return sources;
        }

        public virtual UnityEngine.Object[] Load(int targetRegion)
        {
            _currentRegion = targetRegion;

            int indexOfEnumStart, indexOfEnumEnd;

            EnumHelper.ClapIndexOfEnum(EnumValue.GetType(), _namingStart, _namingEnd, out indexOfEnumStart, out indexOfEnumEnd);

            int clapedLength = indexOfEnumEnd - indexOfEnumStart + 1;

            Enum[] namings = new Enum[clapedLength];
            Array enumTemp = Enum.GetValues(EnumValue.GetType());
            Array.Copy(enumTemp, indexOfEnumStart, namings, 0, clapedLength);


            UnityEngine.Object[] sources = new UnityEngine.Object[clapedLength];

            for (int i = 0; i < enumTemp.Length; i++)
            {
                UnityEngine.Object source = ResourcesExtension.LoadSubDirectory<UnityEngine.Object>(ResourcePathPrefix, enumTemp.GetValue(i).ToString()) as UnityEngine.Object;
                CurrentResourcesDict.Add(enumTemp.GetValue(i) as Enum, source);
                sources[i] = source;
            }

            return sources;
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

        private T Get<T>(Enum sourceName) where T : UnityEngine.Object
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