using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = System.Object;

[CreateAssetMenu(fileName = "DefaultDevGameStart", menuName = "ScriptableObjects/게임시작 툴")]
public class DevGameStartData : ScriptableObject
{
    public const string DEFAULT_SETTING_FILE = "DefaultDevGameStart.asset";
    public const string DEFAULT_SETTING_FOLDER = "Assets/Settings/Resources/";
    public const string DEFAULT_SETTING_PATH = DEFAULT_SETTING_FOLDER + DEFAULT_SETTING_FILE;
    
    private static DevGameStartData _default;
    public static DevGameStartData Default
    {
        get
        {
            if (_default == null)
            {
#if UNITY_EDITOR
                _default = AssetDatabase.LoadAssetAtPath<DevGameStartData>(DEFAULT_SETTING_PATH);
            
                if (_default == null)
                {
                    if (!AssetDatabase.IsValidFolder(DEFAULT_SETTING_FOLDER))
                    {
                        AssetDatabase.CreateFolder("Assets", "Settings");
                        AssetDatabase.CreateFolder("Assets/Settings", "Resources");
                    }
                    
                    _default = CreateInstance<DevGameStartData>();
                    AssetDatabase.CreateAsset(_default, DEFAULT_SETTING_PATH);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
#else
                _default = Resources.Load<DevGameStart>(DEFAULT_SETTING_FILE);
#endif
            }

            return _default;
        }
    }

    public const string LastEditingScenePrefsKey = "LastEditingScenePath";
    private static bool _enterPlayModeEnabled;
    private static EnterPlayModeOptions _enterPlayModeOption;

    [MenuItem("Tools/테스트시작 &o", priority = 0)]
    public static void PlayOnStartScene()
    {
        if (Default.SceneList.Count == 0 || Default.SceneList[0] == null)
        {
            return;
        }
        
        Play(Default.SceneList[0]);
    }
    
    public static void Play(SceneAsset scene)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }
            
        EditorPrefs.SetString(LastEditingScenePrefsKey, EditorSceneManager.GetActiveScene().path);

        _enterPlayModeEnabled = EditorSettings.enterPlayModeOptionsEnabled;
        _enterPlayModeOption = EditorSettings.enterPlayModeOptions;

        // EditorSettings.enterPlayModeOptionsEnabled = true;
        // EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        EditorApplication.playModeStateChanged += OnExitPlay;

        string path = AssetDatabase.GetAssetPath(scene);
        EditorSceneManager.OpenScene(path);
        // EditorUtility.RequestScriptReload();
        EditorApplication.isPlaying = true;
    }

    private static void OnExitPlay(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode)
        {
            return;
        }
        
        string path = EditorPrefs.GetString(LastEditingScenePrefsKey);

        EditorSceneManager.OpenScene(path);
    
        // EditorSettings.enterPlayModeOptionsEnabled = _enterPlayModeEnabled;
        // EditorSettings.enterPlayModeOptions = _enterPlayModeOption;
        EditorApplication.playModeStateChanged -= OnExitPlay;
    }
    
    public List<SceneAsset> SceneList;
}
