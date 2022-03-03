using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Utility
{
    [CustomEditor(typeof(SoundManager))]
    public class SoundManagerEditor : Editor
    {
        SerializedProperty EnumNameCorrect;
        SerializedProperty EnumTypeName;
        SerializedProperty EnumString;
        SerializedProperty ShowSettingsInEditor;
        SerializedProperty ShowPathSceneInEditor;
        SerializedProperty ShowDatasInEditor;
        SerializedProperty ShowPreloadedClipsInEditor;
        SerializedProperty ShowSharedClipsInEditor;
        SerializedProperty ShowCurrentClipsInEditor;
        SerializedProperty ShowPlayingSourcesInEditor;
        SerializedProperty NamingInterval;
        SerializedProperty WorldAudioSourceCount;
        SerializedProperty SharingNamingRegion;
        SerializedProperty SharingSoundsPath;
        SerializedProperty ResourcePathPrefix;
        SerializedProperty UsePathSceneSync;
        SerializedProperty BackGroundMusicClip;
        // Todo
        SerializedProperty ResourcePathsForEachScene;
        SerializedProperty PreloadedAudioClipList;
        
        void OnEnable()
        {
            EnumNameCorrect = serializedObject.FindProperty("EnumNameCorrect");
            EnumTypeName = serializedObject.FindProperty("_enumTypeName");
            EnumString = serializedObject.FindProperty("_enumString");
            ShowSettingsInEditor = serializedObject.FindProperty("ShowSettingsInEditor");
            ShowPathSceneInEditor = serializedObject.FindProperty("ShowPathSceneInEditor");
            ShowDatasInEditor = serializedObject.FindProperty("ShowDatasInEditor");
            ShowPreloadedClipsInEditor = serializedObject.FindProperty("ShowPreloadedClipsInEditor");
            ShowSharedClipsInEditor = serializedObject.FindProperty("ShowSharedClipsInEditor");
            ShowCurrentClipsInEditor = serializedObject.FindProperty("ShowCurrentClipsInEditor");
            ShowPlayingSourcesInEditor = serializedObject.FindProperty("ShowPlayingSourcesInEditor");
            NamingInterval = serializedObject.FindProperty("NamingInterval");
            WorldAudioSourceCount = serializedObject.FindProperty("WorldAudioSourceCount");
            SharingNamingRegion = serializedObject.FindProperty("SharingNamingRegion");
            SharingSoundsPath = serializedObject.FindProperty("SharingSoundsPath");
            ResourcePathPrefix = serializedObject.FindProperty("ResourcePathPrefix");
            UsePathSceneSync = serializedObject.FindProperty("UsePathSceneSync");
            BackGroundMusicClip = serializedObject.FindProperty("BackGroundMusicClip");
            ResourcePathsForEachScene = serializedObject.FindProperty("ResourcePathsForEachScene");
            PreloadedAudioClipList = serializedObject.FindProperty("PreloadedAudioClipList");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SoundManager SM = target as SoundManager;

            if (!EnumNameCorrect.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(EnumTypeName, new GUIContent("Enum Name"));
                if (GUILayout.Button("Submit", GUILayout.Width(50)) || Event.current.keyCode == KeyCode.Return)
                {
                    Type type = EnumHelper.GetEnumType(EnumTypeName.stringValue);
                    
                    if (type != null)
                    {
                        EnumNameCorrect.boolValue = true;
                        EnumString.stringValue = (Activator.CreateInstance(type) as Enum).ToString();
                    }
                }
                EditorGUILayout.EndHorizontal();
            } else
            {
                if (!EditorApplication.isPlaying)
                {
                    if (GUILayout.Button(EnumTypeName.stringValue + " : Reset EnumTypeName")) 
                    {
                        CustomReset();
                        serializedObject.ApplyModifiedProperties();
                        return;
                    }
                    if (GUILayout.Button("Settings")) ShowSettingsInEditor.boolValue = !ShowSettingsInEditor.boolValue;
                    
                    if (ShowSettingsInEditor.boolValue)
                    {
                        EditorGUILayout.PropertyField(NamingInterval, new GUIContent("Enum Region Interval"));
                        EditorGUILayout.PropertyField(WorldAudioSourceCount, new GUIContent("World Audio Source Number"));
                        EditorGUILayout.PropertyField(SharingNamingRegion, new GUIContent("Sharing Naming Region"));
                        EditorGUILayout.PropertyField(SharingSoundsPath, new GUIContent("Sharing Sound Path"));
                        EditorGUILayout.PropertyField(ResourcePathPrefix, new GUIContent("Sound Assets Folder"));

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
                    EditorGUILayout.Space(20);
                }

                ShowDatasInEditor.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(ShowDatasInEditor.boolValue, "Datas");
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (ShowDatasInEditor.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(BackGroundMusicClip, new GUIContent("BackGroundMusic"));
                    if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20))) BackGroundMusicClip.objectReferenceValue = null;
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    ShowPreloadedClipsInEditor.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(ShowPreloadedClipsInEditor.boolValue, "Preloaded Clips");
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    if (GUILayout.Button("Add", GUILayout.Width(100)))
                    {
                        PreloadedAudioClipList.InsertArrayElementAtIndex(PreloadedAudioClipList.arraySize);
                        ShowPreloadedClipsInEditor.boolValue = true;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (ShowPreloadedClipsInEditor.boolValue)
                    {
                        ShowPreloadedEnumClip();
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        EditorGUILayout.EndHorizontal();
                    }

                    if (EditorApplication.isPlaying)
                    {
                        ShowSharedClipsInEditor.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(ShowSharedClipsInEditor.boolValue, "Shared Clips");
                        EditorGUILayout.EndFoldoutHeaderGroup();
                        if (ShowSharedClipsInEditor.boolValue)
                        {
                            ShowSharedEnumClip();
                        }

                        ShowCurrentClipsInEditor.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(ShowCurrentClipsInEditor.boolValue, "Current Clips");
                        EditorGUILayout.EndFoldoutHeaderGroup();
                        if (ShowCurrentClipsInEditor.boolValue)
                        {
                            ShowCurrentEnumClip();
                        }

                        ShowPlayingSourcesInEditor.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(ShowPlayingSourcesInEditor.boolValue, "Playing Sources");
                        EditorGUILayout.EndFoldoutHeaderGroup();
                        if (ShowPlayingSourcesInEditor.boolValue)
                        {
                            ShowPlayingEnumSource();
                        }
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();

            void ShowPreloadedEnumClip()
            {
                for (int i = 0; i < PreloadedAudioClipList.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(PreloadedAudioClipList.GetArrayElementAtIndex(i));
                    if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20))) PreloadedAudioClipList.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                }
            }

            void ShowSharedEnumClip()
            {
                if (SM.SharedAudioClipDict.Count == 0)
                {
                    EditorGUILayout.LabelField("No Shared Clips");
                }

                foreach (KeyValuePair<Enum, AudioClip> pair in SM.SharedAudioClipDict)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Box(pair.Key.ToString(), GUILayout.Width(150));
                    EditorGUILayout.ObjectField(pair.Value, typeof(AudioClip), false);
                    EditorGUILayout.EndHorizontal();
                }
            }

            void ShowCurrentEnumClip()
            {
                if (SM.CurrentAudioClipDict.Count == 0)
                {
                    EditorGUILayout.LabelField("No Current Clips");
                }

                foreach (KeyValuePair<Enum, AudioClip> pair in SM.CurrentAudioClipDict)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Box(pair.Key.ToString(), GUILayout.Width(150));
                    EditorGUILayout.ObjectField(pair.Value, typeof(AudioClip), false);
                    EditorGUILayout.EndHorizontal();
                }
            }

            void ShowPlayingEnumSource()
            {
                if (SM.PlayingAudioSourceDict.Count == 0)
                {
                    EditorGUILayout.LabelField("No Playing Clips");
                }

                foreach (KeyValuePair<Enum, AudioSource> pair in SM.PlayingAudioSourceDict)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Box(pair.Key.ToString(), GUILayout.Width(150));
                    EditorGUILayout.ObjectField(pair.Value, typeof(AudioSource), false);
                    EditorGUILayout.EndHorizontal();
                }
            }

            void CustomReset()
            {
                EnumNameCorrect.boolValue = false;
                EnumTypeName.stringValue = string.Empty;
                EnumString.stringValue = string.Empty;
            }
        }
    }
}