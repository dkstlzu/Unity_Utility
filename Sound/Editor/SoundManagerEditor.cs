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
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SoundManager SM = target as SoundManager;

            if (!SM.EnumNameCorrect)
            {
                EditorGUILayout.BeginHorizontal();
                SM.EnumName = EditorGUILayout.TextField("Enum Name", SM.EnumName);
                if (GUILayout.Button("Submit", GUILayout.Width(50)))
                {
                    Type type = EnumHelper.GetEnumType(SM.EnumName);
                    
                    if (type != null)
                    {
                        SM.EnumNameCorrect = true;
                        SM.EnumValue = Activator.CreateInstance(type) as Enum;
                    }
                }
                EditorGUILayout.EndHorizontal();
            } else
            {
                if (!EditorApplication.isPlaying)
                {
                    if (GUILayout.Button(SM.EnumName + " : Reset EnumName")) CustomReset();
                    if (GUILayout.Button("Settings")) SM.ShowSettingsInEditor = !SM.ShowSettingsInEditor;
                    
                    if (SM.ShowSettingsInEditor)
                    {
                        SM.NamingInterval = EditorGUILayout.IntField("Enum Region Interval", SM.NamingInterval);
                        SM.WorldAudioSourceCount = EditorGUILayout.IntField("World Audio Source Number", SM.WorldAudioSourceCount);
                        SM.SharingNamingRegion = EditorGUILayout.IntField("Sharing Naming Region", SM.SharingNamingRegion);
                        SM.SharingSoundsPath = EditorGUILayout.TextField("Sharing Sound Path", SM.SharingSoundsPath);
                        SM.ResourcePathPrefix = EditorGUILayout.TextField("Sound Assets Folder", SM.ResourcePathPrefix);

                        SM.UsePathSceneSync = EditorGUILayout.Toggle("Use Path Scene Auto Load", SM.UsePathSceneSync);

                        if (SM.UsePathSceneSync)
                        {
                            EditorGUILayout.BeginHorizontal();
                            SM.ShowPathSceneInEditor = EditorGUILayout.BeginFoldoutHeaderGroup(SM.ShowPathSceneInEditor, "Path-Scene");
                            EditorGUILayout.EndFoldoutHeaderGroup();
                            if (GUILayout.Button("Add", GUILayout.Width(100)))
                            {
                                SM.ResourcePathsForEachScene.Add(("", ""));
                                SM.ShowPathSceneInEditor = true;
                            }
                            EditorGUILayout.EndHorizontal();

                            if (SM.ShowPathSceneInEditor)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.Space(20);
                                EditorGUILayout.LabelField("Path", GUILayout.Width(150));
                                EditorGUILayout.LabelField("Scene", GUILayout.Width(150));
                                EditorGUILayout.EndHorizontal();

                                for (int i = 0; i < SM.ResourcePathsForEachScene.Count; i++)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(EditorGUIUtility.currentViewWidth/10));
                                    SM.ResourcePathsForEachScene[i] = 
                                        (EditorGUILayout.TextField(SM.ResourcePathsForEachScene[i].Path), EditorGUILayout.TextField(SM.ResourcePathsForEachScene[i].Scene));
                                    if (GUILayout.Button("x", GUILayout.Width(EditorGUIUtility.currentViewWidth/10)))
                                    {
                                        SM.ResourcePathsForEachScene.RemoveAt(i);
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                        }
                    }
                    EditorGUILayout.Space(20);
                }

                SM.ShowDatasInEditor = EditorGUILayout.BeginFoldoutHeaderGroup(SM.ShowDatasInEditor, "Datas");
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (SM.ShowDatasInEditor)
                {
                    EditorGUILayout.BeginHorizontal();
                    SM.BackGroundMusicClip = EditorGUILayout.ObjectField(SM.BackGroundMusicClip, typeof(AudioClip), true) as AudioClip;
                    if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20))) SM.BackGroundMusicClip = null;
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    SM.ShowPreloadedClipsInEditor = EditorGUILayout.BeginFoldoutHeaderGroup(SM.ShowPreloadedClipsInEditor, "Preloaded Clips");
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    if (GUILayout.Button("Add", GUILayout.Width(100)))
                    {
                        SM.PreloadedAudioClipList.Add(null);
                        SM.ShowPreloadedClipsInEditor = true;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (SM.ShowPreloadedClipsInEditor)
                    {
                        ShowPreloadedEnumClip();
                    }

                    if (EditorApplication.isPlaying)
                    {
                        SM.ShowSharedClipsInEditor = EditorGUILayout.BeginFoldoutHeaderGroup(SM.ShowSharedClipsInEditor, "Shared Clips");
                        EditorGUILayout.EndFoldoutHeaderGroup();
                        if (SM.ShowSharedClipsInEditor)
                        {
                            ShowSharedEnumClip();
                        }

                        SM.ShowCurrentClipsInEditor = EditorGUILayout.BeginFoldoutHeaderGroup(SM.ShowCurrentClipsInEditor, "Current Clips");
                        EditorGUILayout.EndFoldoutHeaderGroup();
                        if (SM.ShowCurrentClipsInEditor)
                        {
                            ShowCurrentEnumClip();
                        }

                        SM.ShowPlayingSourcesInEditor = EditorGUILayout.BeginFoldoutHeaderGroup(SM.ShowPlayingSourcesInEditor, "Playing Sources");
                        EditorGUILayout.EndFoldoutHeaderGroup();
                        if (SM.ShowPlayingSourcesInEditor)
                        {
                            ShowPlayingEnumSource();
                        }
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();

            void ShowPreloadedEnumClip()
            {
                for (int i = 0; i < SM.PreloadedAudioClipList.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    SM.PreloadedAudioClipList[i] = EditorGUILayout.ObjectField(SM.PreloadedAudioClipList[i], typeof(AudioClip), true) as AudioClip;
                    if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20))) SM.PreloadedAudioClipList.RemoveAt(i);
                    EditorGUILayout.EndHorizontal();
                }
            }

            void ShowSharedEnumClip()
            {
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
                SM.gameObject.AddComponent<SoundManager>();
                DestroyImmediate(SM);
            }
        }
    }
}