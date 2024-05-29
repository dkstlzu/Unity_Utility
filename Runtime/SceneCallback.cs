using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace dkstlzu.Utility
{
    [Serializable]
    public struct SceneCallbackEventHandler
    {
#if UNITY_EDITOR
        public SceneAsset Scene;
#endif
        [SerializeField]
        private string _sceneName;
        public string SceneName
        {
            get => _sceneName;
            set => _sceneName = value;
        }
        
        public UnityEvent OnLoad;
        public UnityEvent OnUnload;
    }
    
    [Serializable]
    public class SceneCallback
    {
        public static bool Initiated;

        public static Dictionary<string, Action> OnLoadCallBackDict = new Dictionary<string, Action>();
        public static Dictionary<string, Action> OnUnloadCallBackDict = new Dictionary<string, Action>();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            if (Initiated) return;

            Initiated = true;

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

#if UNITY_EDITOR
            // Editor Reload domain = false 사용을 위한 코드입니다.
            EditorApplication.playModeStateChanged += RemoveStaticEventHandlerOnStopPlay;
#endif
        }

#if UNITY_EDITOR
        private static void RemoveStaticEventHandlerOnStopPlay(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                SceneManager.sceneUnloaded -= OnSceneUnloaded;

                EditorApplication.playModeStateChanged -= RemoveStaticEventHandlerOnStopPlay;
                Initiated = false;
                
                OnLoadCallBackDict.Clear();
                OnUnloadCallBackDict.Clear();
            }
        }
#endif

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (OnLoadCallBackDict.TryGetValue(scene.name, out Action action))
            {
                action?.Invoke();
            }
        }

        static void OnSceneUnloaded(Scene scene)
        {
            if (OnUnloadCallBackDict.TryGetValue(scene.name, out Action action))
            {
                action?.Invoke();
            }
        }

        public SceneCallbackEventHandler[] Callbacks;

        public SceneCallback(IEnumerable<string> sceneNames)
        {
            foreach(string sceneName in sceneNames)
            {
                OnLoadCallBackDict.TryAdd(sceneName, delegate{});
                OnUnloadCallBackDict.TryAdd(sceneName, delegate{});
            }

            Init();
        }

        public static SceneCallback GetEditorBuildTargetSceneCallback()
        {
            List<string> sceneNames = new List<string>();

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                sceneNames.Add(SceneManager.GetSceneByBuildIndex(i).name);
            }

            return new SceneCallback(sceneNames);
        }

        public void RegisterEvents()
        {
            foreach (SceneCallbackEventHandler handler in Callbacks)
            {
                OnLoadCallBackDict.TryAdd(handler.SceneName, delegate{});
                OnUnloadCallBackDict.TryAdd(handler.SceneName, delegate{});

                OnLoadCallBackDict[handler.SceneName] += handler.OnLoad.Invoke;
                OnUnloadCallBackDict[handler.SceneName] += handler.OnUnload.Invoke;
            }
            
            Init();
        }
    }
}