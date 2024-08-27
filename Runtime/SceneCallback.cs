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
    public class SceneCallbackEventHandler
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

        public void RegisterEvent()
        {
            SceneCallback.RegisterLoadEvent(SceneName, OnLoad.Invoke);
            SceneCallback.RegisterUnloadEvent(SceneName, OnUnload.Invoke);
        }

        public void UnregisterEvent()
        {
            SceneCallback.UnregisterLoadEvent(SceneName, OnLoad.Invoke);
            SceneCallback.UnregisterUnloadEvent(SceneName, OnUnload.Invoke);
        }
    }
    
    [Serializable]
    public class SceneCallback
    {
        public static bool Initiated { get; private set; }

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

        public static void RegisterLoadEvent(string sceneName, Action action)
        {
            OnLoadCallBackDict.TryAdd(sceneName, delegate { });

            OnLoadCallBackDict[sceneName] += action;
        }

        public static void UnregisterLoadEvent(string sceneName, Action action)
        {
            // Dictionary.TryGetValue() 함수 썼다가 TValue : Action 의 값복사가 일어나서 제대로 구독해제가 안되는 경우가 있었음
            if (OnLoadCallBackDict.ContainsKey(sceneName))
            {
                OnLoadCallBackDict[sceneName] -= action;
                if (OnLoadCallBackDict[sceneName].GetInvocationList().Length == 0)
                {
                    OnLoadCallBackDict.Remove(sceneName);
                }
            }
        }
        
        public static void RegisterUnloadEvent(string sceneName, Action action)
        {
            OnUnloadCallBackDict.TryAdd(sceneName, delegate { });

            OnUnloadCallBackDict[sceneName] += action;
        }

        public static void UnregisterUnloadEvent(string sceneName, Action action)
        {
            if (OnUnloadCallBackDict.ContainsKey(sceneName))
            {
                OnUnloadCallBackDict[sceneName] -= action;
                if (OnUnloadCallBackDict[sceneName].GetInvocationList().Length == 0)
                {
                    OnUnloadCallBackDict.Remove(sceneName);
                }
            }
        }

        public void RegisterEvents()
        {
            foreach (SceneCallbackEventHandler handler in Callbacks)
            {
                handler.RegisterEvent();
            }
            
            Init();
        }

        public void UnregisterEvents()
        {
            foreach (SceneCallbackEventHandler handler in Callbacks)
            {
                handler.UnregisterEvent();
            }
        }
    }
}