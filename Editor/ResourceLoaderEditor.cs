using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Utility
{
    [CustomEditor(typeof(ResourceLoader))]
    public class ResourceLoaderEditor : EnumSettableEditor
    {
        SerializedProperty ShowSettingsInEditor;
        SerializedProperty ShowPathSceneInEditor;
        SerializedProperty ShowDatasInEditor;
        SerializedProperty ShowPreloadedResourcesInEditor;
        SerializedProperty ShowSharedResourcesInEditor;
        SerializedProperty ShowCurrentResourcesInEditor;
        SerializedProperty ShowResourcesSubDirectoryCache;

        SerializedProperty NamingInterval;
        SerializedProperty SharingResourcesPath;
        SerializedProperty ResourcePathPrefix;
        SerializedProperty UsePathSceneSync;

        SerializedProperty ResourcePathsForEachScene;
        SerializedProperty PreloadedResourcesList;
        SerializedProperty ResourceSubDirectories;

        private ResourceLoader GetTarget()
        {
            return target as ResourceLoader;
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            ShowSettingsInEditor = serializedObject.FindProperty("ShowSettingsInEditor");
            ShowPathSceneInEditor = serializedObject.FindProperty("ShowPathSceneInEditor");
            ShowDatasInEditor = serializedObject.FindProperty("ShowDatasInEditor");
            ShowPreloadedResourcesInEditor = serializedObject.FindProperty("ShowPreloadedResourcesInEditor");
            ShowSharedResourcesInEditor = serializedObject.FindProperty("ShowSharedResourcesInEditor");
            ShowCurrentResourcesInEditor = serializedObject.FindProperty("ShowCurrentResourcesInEditor");
            ShowResourcesSubDirectoryCache = serializedObject.FindProperty("ShowResourcesSubDirectoryCache");

            NamingInterval = serializedObject.FindProperty("NamingInterval");
            SharingResourcesPath = serializedObject.FindProperty("SharingResourcesPath");
            ResourcePathPrefix = serializedObject.FindProperty("ResourcePathPrefix");
            UsePathSceneSync = serializedObject.FindProperty("UsePathSceneSync");

            ResourcePathsForEachScene = serializedObject.FindProperty("ResourcePathsForEachScene");
            PreloadedResourcesList = serializedObject.FindProperty("PreloadedResourcesList");
            ResourceSubDirectories = serializedObject.FindProperty("ResourceSubDirectories");
        }

        protected override void OnNotPlayingInspectorGUI()
        {
            base.OnNotPlayingInspectorGUI();
            if (GUILayout.Button("Settings")) ShowSettingsInEditor.boolValue = !ShowSettingsInEditor.boolValue;
            
            if (ShowSettingsInEditor.boolValue)
            {
                EditorGUILayout.PropertyField(ResourcePathPrefix, new GUIContent("Path Prefix"));
                EditorGUILayout.PropertyField(SharingResourcesPath, new GUIContent("Sharing Source Path"));
                EditorGUILayout.PropertyField(NamingInterval, new GUIContent("Enum Region Interval"));

                EditorGUILayout.PropertyField(UsePathSceneSync, new GUIContent("Use Path Scene Auto Load"));

                if (UsePathSceneSync.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    ShowPathSceneInEditor.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(ShowPathSceneInEditor.boolValue, "Path-Scene");
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    if (GUILayout.Button("Add", GUILayout.Width(100)))
                    {
                        ResourcePathsForEachScene.InsertArrayElementAtIndex(ResourcePathsForEachScene.arraySize);
                        ShowPathSceneInEditor.boolValue = true;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (ShowPathSceneInEditor.boolValue)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.Space(20);
                        EditorGUILayout.LabelField("Path", GUILayout.Width(EditorGUIUtility.currentViewWidth/2));
                        EditorGUILayout.LabelField("Scene", GUILayout.Width(EditorGUIUtility.currentViewWidth/2));
                        EditorGUILayout.EndHorizontal();

                        for (int i = 0; i < ResourcePathsForEachScene.arraySize; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(EditorGUIUtility.currentViewWidth/10));
                            ResourcePathsForEachScene.GetArrayElementAtIndex(i).FindPropertyRelative("Path").stringValue = EditorGUILayout.TextField(ResourcePathsForEachScene.GetArrayElementAtIndex(i).FindPropertyRelative("Path").stringValue);
                            ResourcePathsForEachScene.GetArrayElementAtIndex(i).FindPropertyRelative("Scene").stringValue = EditorGUILayout.TextField(ResourcePathsForEachScene.GetArrayElementAtIndex(i).FindPropertyRelative("Scene").stringValue);
                            if (GUILayout.Button("x", GUILayout.Width(EditorGUIUtility.currentViewWidth/10)))
                            {
                                ResourcePathsForEachScene.DeleteArrayElementAtIndex(i);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }

            EditorGUILayout.BeginHorizontal();
            ShowResourcesSubDirectoryCache.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(ShowResourcesSubDirectoryCache.boolValue, "SubDirectory Cache");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (GUILayout.Button("Search", GUILayout.Width(100)))
            {
                ResourceSubDirectories.ClearArray();

                string ResourcesPath = Application.dataPath + "/Resources";
                string[] directories = Directory.GetDirectories(ResourcesPath, "*", SearchOption.AllDirectories);
                foreach (var item in directories)
                {
                    string itemPath = item.Substring(ResourcesPath.Length + 1);
                    MonoBehaviour.print($"{GetType()} : " + itemPath);
                    ResourceSubDirectories.InsertArrayElementAtIndex(ResourceSubDirectories.arraySize);
                    ResourceSubDirectories.GetArrayElementAtIndex(ResourceSubDirectories.arraySize - 1).stringValue = itemPath;
                }
            }
            EditorGUILayout.EndHorizontal();
            if (ShowResourcesSubDirectoryCache.boolValue)
            {
                ShowSubDirectoryCache();
            }
        }

        protected override void OnPlayingInspectorGUI()
        {
            ShowSharedResourcesInEditor.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(ShowSharedResourcesInEditor.boolValue, "Shared Sources");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (ShowSharedResourcesInEditor.boolValue)
            {
                ShowSharedClips();
            }

            ShowCurrentResourcesInEditor.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(ShowCurrentResourcesInEditor.boolValue, "Current Sources");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (ShowCurrentResourcesInEditor.boolValue)
            {
                ShowCurrentClips();
            }
        }

        protected override void OnOverridingInspectorGUI()
        {
            base.OnOverridingInspectorGUI();
            ShowDatasInEditor.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(ShowDatasInEditor.boolValue, "Datas");
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (ShowDatasInEditor.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                ShowPreloadedResourcesInEditor.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(ShowPreloadedResourcesInEditor.boolValue, "Preloaded Sources");
                EditorGUILayout.EndFoldoutHeaderGroup();
                if (GUILayout.Button("Add", GUILayout.Width(100)))
                {
                    PreloadedResourcesList.InsertArrayElementAtIndex(PreloadedResourcesList.arraySize);
                    ShowPreloadedResourcesInEditor.boolValue = true;
                }
                EditorGUILayout.EndHorizontal();

                // ---------------------------------------------

                if (ShowPreloadedResourcesInEditor.boolValue)
                {
                    ShowPreloadedEnumSource();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        void ShowSubDirectoryCache()
        {
            for (int i = 0; i < ResourceSubDirectories.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                ResourceSubDirectories.GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(ResourceSubDirectories.GetArrayElementAtIndex(i).stringValue);
                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20))) ResourceSubDirectories.DeleteArrayElementAtIndex(i);
                EditorGUILayout.EndHorizontal();
            }
        }

        void ShowPreloadedEnumSource()
        {
            for (int i = 0; i < PreloadedResourcesList.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(PreloadedResourcesList.GetArrayElementAtIndex(i));
                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20))) PreloadedResourcesList.DeleteArrayElementAtIndex(i);
                EditorGUILayout.EndHorizontal();
            }
        }

        void ShowSharedClips()
        {
            if (GetTarget().SharedResourcesDict.Count == 0)
            {
                EditorGUILayout.LabelField("No Shared Sources");
            }

            foreach (KeyValuePair<Enum, UnityEngine.Object> pair in GetTarget().SharedResourcesDict)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Box(pair.Key.ToString(), GUILayout.Width(150));
                EditorGUILayout.ObjectField(pair.Value, typeof(UnityEngine.AudioClip), false);
                EditorGUILayout.EndHorizontal();
            }
        }

        void ShowCurrentClips()
        {
            if (GetTarget().CurrentResourcesDict.Count == 0)
            {
                EditorGUILayout.LabelField("No Current Sources");
            }

            foreach (KeyValuePair<Enum, UnityEngine.Object> pair in GetTarget().CurrentResourcesDict)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Box(pair.Key.ToString(), GUILayout.Width(150));
                EditorGUILayout.ObjectField(pair.Value, typeof(UnityEngine.AudioClip), false);
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}