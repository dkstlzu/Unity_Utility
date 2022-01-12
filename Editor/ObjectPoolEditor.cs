using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Utility
{
    [CustomEditor(typeof(ObjectPool))]
    public class ObjectPoolEditor : Editor
    {
        dynamic enumPopUp;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ObjectPool PL = target as ObjectPool;

            if (GUILayout.Button("TEST"))
            {
                SerializedProperty property = serializedObject.GetIterator();
                UnityConsole.ClearConsole();
                while(property.Next(true))
                {
                    MonoBehaviour.print(property.propertyPath);
                }
            }

            PL.ShowStaticEnumsInEditor = EditorGUILayout.BeginFoldoutHeaderGroup(PL.ShowStaticEnumsInEditor, "Static Included Enums");
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (PL.ShowStaticEnumsInEditor)
            {
                int index = 1;
                foreach(var v in ObjectPool.IncludedEnumsDict)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(index + ". " +v.Key);
                    EditorGUILayout.LabelField("Count : " + v.Value.ToString(), GUILayout.Width(100));
                    EditorGUILayout.EndHorizontal();
                    index++;
                }
            }

            if (!PL.EnumNameCorrect)
            {
                EditorGUILayout.BeginHorizontal();
                PL.EnumName = EditorGUILayout.TextField("Enum Name", PL.EnumName);
                if (GUILayout.Button("Submit", GUILayout.Width(50)))
                {
                    Type type = EnumHelper.GetEnumType(PL.EnumName);
                    
                    if (type != null)
                    {
                        PL.EnumNameCorrect = true;
                        PL.PoolEnum = Activator.CreateInstance(type) as Enum;
                        int countTemp;
                        if (ObjectPool.IncludedEnumsDict.TryGetValue(PL.PoolEnum, out countTemp))
                        {
                            ObjectPool.IncludedEnumsDict[PL.PoolEnum] = countTemp+1;
                        } else
                        {
                            ObjectPool.IncludedEnumsDict.Add(PL.PoolEnum, 1);
                        }
                        PL.ShowSettingsInEditor = true;
                    }
                }
                EditorGUILayout.EndHorizontal();
            } else
            {
                // MonoBehaviour.print(serializedObject.FindProperty());
                if (!EditorApplication.isPlaying)
                {
                    if (GUILayout.Button("Settings")) PL.ShowSettingsInEditor = !PL.ShowSettingsInEditor;

                    if (PL.ShowSettingsInEditor)
                    {
                        enumPopUp = PL.PoolEnum;
                        if (enumPopUp != null)
                            enumPopUp = EditorGUILayout.EnumPopup("PoolType", enumPopUp);
                        PL.PoolEnum = enumPopUp;
                        // PL.PoolEnum = EditorGUILayout.EnumPopup("PoolType", PL.PoolEnum);
                        if (!PL.SourceObject)
                            PL.SoruceFilePath = EditorGUILayout.TextField("Source File Path", PL.SoruceFilePath);
                        if (PL.SoruceFilePath.Equals(string.Empty))
                            PL.SourceObject = EditorGUILayout.ObjectField("Source Prefab", PL.SourceObject, typeof(UnityEngine.GameObject), true) as UnityEngine.GameObject;
                        PL.PoolSize = EditorGUILayout.IntField("PoolSize", PL.PoolSize);
                        PL.AutoReturn = EditorGUILayout.Toggle("Use AutoReturn", PL.AutoReturn);
                        if (PL.AutoReturn)
                        {
                            PL.AutoReturnTime = (float)EditorGUILayout.Slider(new GUIContent("Auto Return Time", "Use Seconds"), PL.AutoReturnTime, 0, 60);
                        }

                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Reset this pool")) CustomReset();
                        GUI.enabled = PL.SourceObject || !PL.SoruceFilePath.Equals(string.Empty);
                        if (!PL.isAllocated)
                        {
                            if (GUILayout.Button("Allocate", GUILayout.Width(EditorGUIUtility.currentViewWidth/2)))
                            {
                                if (!PL.SourceObject)
                                    PL.SourceObject = Resources.Load(PL.SoruceFilePath) as GameObject;
                                PL.Allocate();
                            }
                        } else if (GUILayout.Button("Destroy Objects", GUILayout.Width(EditorGUIUtility.currentViewWidth/2)))
                        {
                            DestoryObjects();
                        }
                        GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space(20);
                    }
                }

                PL.ShowDatasInEditor = EditorGUILayout.BeginFoldoutHeaderGroup(PL.ShowDatasInEditor, "Datas");
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (PL.ShowDatasInEditor)
                {
                    EditorGUILayout.LabelField("Available Objects");
                    ShowAvilableObjects();
                    EditorGUILayout.LabelField("Active Objects");
                    ShowActiveObjects();
                }
            }


            serializedObject.ApplyModifiedProperties();

            void CustomReset()
            {
                if (ObjectPool.IncludedEnumsDict.ContainsKey(PL.PoolEnum))
                {
                    ObjectPool.IncludedEnumsDict[PL.PoolEnum] = ObjectPool.IncludedEnumsDict[PL.PoolEnum]-1;
                    if (ObjectPool.IncludedEnumsDict[PL.PoolEnum] <= 0)
                    {
                        ObjectPool.IncludedEnumsDict.Remove(PL.PoolEnum);
                    }
                    DestoryObjects();
                }
                PL.gameObject.AddComponent<ObjectPool>();
                DestroyImmediate(PL);
            }

            void DestoryObjects()
            {
                PL.Clear();
                PL.isAllocated = false;
            }

            void ShowAvilableObjects()
            {
                foreach (GameObject obj in PL.AvailableObjects)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Box(obj.name, GUILayout.Width(150));
                    EditorGUILayout.ObjectField(obj, typeof(GameObject), false);
                    EditorGUILayout.EndHorizontal();
                }
            }

            void ShowActiveObjects()
            {
                foreach (GameObject obj in PL.ActiveObjects)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Box(obj.name, GUILayout.Width(150));
                    EditorGUILayout.ObjectField(obj, typeof(GameObject), false);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }
}