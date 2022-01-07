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
        bool showSettings;
        bool pathSceneFold;

        bool datasFold;
        bool preFold;
        bool sharedFold;
        bool currentFold;
        bool playingFold;

        List<dynamic> preloadedEnumList = new List<dynamic>();

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
                    // MonoBehaviour.print(SM.EnumName);
                    // MonoBehaviour.print(SM.EnumType);
                    // MonoBehaviour.print(typeof(DocsaSoundNaming));
                    Type type = Type.GetType(SM.EnumName + ", Assembly-CSharp");
                    if (type == null) type = Type.GetType(SM.EnumName + ", Assembly-CSharp-firstpass");
                    
                    if (type != null)
                    {
                        SM.EnumType = type;
                        SM.EnumNameCorrect = true;
                    }
                }
                EditorGUILayout.EndHorizontal();
            } else
            {
                if (!EditorApplication.isPlaying)
                {
                    if (GUILayout.Button("Reset EnumName")) CustomReset();
                    if (GUILayout.Button("Settings")) showSettings = !showSettings;
                    
                    if (showSettings)
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
                            pathSceneFold = EditorGUILayout.BeginFoldoutHeaderGroup(pathSceneFold, "Path-Scene");
                            EditorGUILayout.EndFoldoutHeaderGroup();
                            if (GUILayout.Button("Add", GUILayout.Width(100)))
                            {
                                SM.ResourcePathsForEachScene.Add(new StringTuple());
                                pathSceneFold = true;
                            }
                            EditorGUILayout.EndHorizontal();

                            if (pathSceneFold)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.Space(20);
                                EditorGUILayout.LabelField("Path", GUILayout.Width(150));
                                EditorGUILayout.LabelField("Scene", GUILayout.Width(150));
                                EditorGUILayout.EndHorizontal();

                                for (int i = 0; i < SM.ResourcePathsForEachScene.Count; i++)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(20));
                                    SM.ResourcePathsForEachScene[i].String1 = EditorGUILayout.TextField(SM.ResourcePathsForEachScene[i].String1, GUILayout.Width(150));
                                    SM.ResourcePathsForEachScene[i].String2 = EditorGUILayout.TextField(SM.ResourcePathsForEachScene[i].String2, GUILayout.Width(150));
                                    if (GUILayout.Button("x", GUILayout.Width(20)))
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

                datasFold = EditorGUILayout.BeginFoldoutHeaderGroup(datasFold, "Datas");
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (datasFold)
                {
                    EditorGUILayout.BeginHorizontal(GUIStyle.none);
                    preFold = EditorGUILayout.BeginFoldoutHeaderGroup(preFold, "Preloaded Clips");
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    if (GUILayout.Button("Add", GUILayout.Width(100)))
                    {
                        SM.PreloadedAudioClipList.Add(null);
                        preFold = true;
                    }
                    EditorGUILayout.EndHorizontal();
                    if (preFold)
                    {
                        ShowPreloadedEnumClip();
                    }

                    sharedFold = EditorGUILayout.BeginFoldoutHeaderGroup(sharedFold, "Shared Clips");
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    if (sharedFold)
                    {
                        ShowSharedEnumClip();
                    }

                    currentFold = EditorGUILayout.BeginFoldoutHeaderGroup(currentFold, "Current Clips");
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    if (currentFold)
                    {
                        ShowCurrentEnumClip();
                    }

                    playingFold = EditorGUILayout.BeginFoldoutHeaderGroup(playingFold, "Playing Sources");
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    if (playingFold)
                    {
                        ShowPlayingEnumSource();
                    }

                }
                // Enum.TryParse<MyEnum>(EditorGUI.EnumPopup(new Rect(30, 30, 80, 80), testEnum).ToString(), out testEnum);
                // SM.resourcePathsForEachScene = EditorGUILayout.

                serializedObject.ApplyModifiedProperties();
            }

            void ShowPreloadedEnumClip()
            {
                for (int i = 0; i < SM.PreloadedAudioClipList.Count; i++)
                {
                    SM.PreloadedAudioClipList[i] = EditorGUILayout.ObjectField(SM.PreloadedAudioClipList[i], typeof(AudioClip), true) as AudioClip;
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