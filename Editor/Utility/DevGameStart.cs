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

namespace dkstlzu.Utility
{
    public class DevGameStart : ScriptableObject
    {
        private const string DEFAULT_SETTING_FILE = "DefaultDevGameStart.asset";
        private const string DEFAULT_SETTING_FOLDER = "Assets/Settings/Resources/";
        private const string DEFAULT_SETTING_PATH = DEFAULT_SETTING_FOLDER + DEFAULT_SETTING_FILE;
        
        private static DevGameStart _default;
        public static DevGameStart Default
        {
            get
            {
                if (_default == null)
                {
#if UNITY_EDITOR
                    _default = AssetDatabase.LoadAssetAtPath<DevGameStart>(DEFAULT_SETTING_PATH);
                
                    if (_default == null)
                    {
                        _default = CreateInstance<DevGameStart>();
                        AssetDatabase.CreateAsset(_default, DEFAULT_SETTING_PATH);
                        AssetDatabase.SaveAssets();
                    }
#else
                    _default = Resources.Load<DevGameStart>(DEFAULT_SETTING_FILE);
#endif
                }

                return _default;
            }
        }

        [CustomPropertyDrawer(typeof(Data))]
        class DataDrawer : PropertyDrawer
        {
            private PropertyField _sp;
            private PropertyField _kp;
            private PropertyField _mp;

            private string _sceneName;
            private KeyCombination _keyCombination;
            
            public override VisualElement CreatePropertyGUI(SerializedProperty property)
            {
                VisualElement root = new VisualElement();
                
                var pf = new PropertyField(property);
                root.Add(pf);

                _sp = new PropertyField(property.FindPropertyRelative("Scene"));
                _sp.RegisterValueChangeCallback(SetScene);
                _sceneName = property.FindPropertyRelative("Scene").objectReferenceValue.name;
                root.Add(_sp);

                _kp = new PropertyField(property.FindPropertyRelative("ShortcutKeycode"));
                _kp.RegisterValueChangeCallback(SetShortcut);
                root.Add(_kp);

                _mp = new PropertyField(property.FindPropertyRelative("ShortcutModifiers"));
                _mp.RegisterValueChangeCallback(SetModifier);
                root.Add(_mp);

                _keyCombination = new KeyCombination((KeyCode)property.FindPropertyRelative("ShortcutKeycode").enumValueIndex, (ShortcutModifiers)property.FindPropertyRelative("ShortcutModifiers").enumValueIndex);

                return root;
            }

            private void SetScene(SerializedPropertyChangeEvent evt)
            {
                _sceneName = evt.changedProperty.objectReferenceValue.name;
                Set();
            }
            
            private void SetShortcut(SerializedPropertyChangeEvent evt)
            {
                _keyCombination = new KeyCombination((KeyCode)evt.changedProperty.enumValueIndex, _keyCombination.modifiers);
                Set();
            }
            
            private void SetModifier(SerializedPropertyChangeEvent evt)
            {
                _keyCombination = new KeyCombination(_keyCombination.keyCode, (ShortcutModifiers)evt.changedProperty.enumValueIndex);
                Set();
            }

            private void Set()
            {
                // ShortcutManager.instance.RebindShortcut($"{_sceneName}/Load", new ShortcutBinding(_keyCombination));
            }
        }
        
        [Serializable]
        public class Data
        {
            public SceneAsset Scene;
            public KeyCode ShortcutKeycode;
            public ShortcutModifiers ShortcutModifiers;
            
            public void GenerateFile()
            {
                File.WriteAllText(DEFAULT_SETTING_PATH, JsonUtility.ToJson(this));
                AssetDatabase.Refresh();
            }
        }

        public List<Data> Datas;

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            var provider = new SettingsProvider("Project/DevGameStart", SettingsScope.Project);

            provider.label = "빠른 실행";
            provider.activateHandler += (searchContext, root) =>
            {
                var so = new SerializedObject(Default);
                var property = so.FindProperty(nameof(Datas));
                SerializedPropertyDebug.LogAllPropertyPath(so);
                
                root.Add(new ObjectField("설정파일")
                {
                    objectType = typeof(DevGameStart),
                    value = Default,
                });
                
                for (int i = 0; i < property.arraySize; i++)
                {
                    var sceneProperty = property.GetArrayElementAtIndex(i).FindPropertyRelative("Scene");
                    var keycodeProperty = property.GetArrayElementAtIndex(i).FindPropertyRelative("ShortcutKeycode");
                    var modifiersProperty = property.GetArrayElementAtIndex(i).FindPropertyRelative("ShortcutModifiers");

                    var of = new ObjectField("씬")
                    {
                        objectType = typeof(SceneAsset),
                        value = sceneProperty.objectReferenceValue
                    };
                    root.Add(of);

                    var keycode = new EnumField("키코드", (KeyCode)keycodeProperty.enumValueIndex);
                    root.Add(keycode);
                    var modifiers = new EnumField("키조합", (ShortcutModifiers)modifiersProperty.enumValueIndex);
                    root.Add(modifiers);
                }
            };
        
            return provider;
        }
        
        public const string LastEditingScenePrefsKey = "LastEditingScenePath";
        private static bool _enterPlayModeEnabled;
        private static EnterPlayModeOptions _enterPlayModeOption;
    
        [MenuItem("Dev/테스트시작", priority = 0)]
        [Shortcut("DevPlayStart", KeyCode.O, ShortcutModifiers.Alt)]
        public static void PlayOnStartScene()
        {
            EditorPrefs.SetString(LastEditingScenePrefsKey, EditorSceneManager.GetActiveScene().path);

            _enterPlayModeEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            _enterPlayModeOption = EditorSettings.enterPlayModeOptions;

            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        
            CustomEditorPlayCallBack.OnEnteredEditMode += OnExitPlay;
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene("Assets/Scenes/00 Menu.unity");
            EditorApplication.isPlaying = true;
        }

        private static void OnExitPlay()
        {
            string path = EditorPrefs.GetString(LastEditingScenePrefsKey);

            EditorSceneManager.OpenScene(path);
        
            EditorSettings.enterPlayModeOptionsEnabled = _enterPlayModeEnabled;
            EditorSettings.enterPlayModeOptions = _enterPlayModeOption;
            CustomEditorPlayCallBack.OnEnteredEditMode -= OnExitPlay;
        }
    }
}