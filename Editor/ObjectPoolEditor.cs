using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

namespace Utility
{
    [CustomEditor(typeof(ObjectPool))]
    public class ObjectPoolEditor : Editor
    {
        [MenuItem("ObjectPool/Print Properties")]
        static void PrintProperties()
        {
            UnityConsole.ClearConsole();
            Component[] components = Selection.activeGameObject.GetComponents(typeof(Component));

            foreach(Component component in components)
            {
                if (component is Transform) continue;
                SerializedProperty property = new SerializedObject(component).GetIterator();

                string PropString = "";

                while(property.Next(true))
                {
                    PropString += property.propertyPath + "\n";
                }

                MonoBehaviour.print($"Properties of {component.gameObject.name}.{component.name}");
                MonoBehaviour.print(PropString);
            }
        }

        SerializedProperty EnumName;
        SerializedProperty EnumValue;
        SerializedProperty EnumNameCorrect;
        SerializedProperty SourceObject;
        SerializedProperty SourceFilePath;
        SerializedProperty PoolSize;
        SerializedProperty AutoReturn;
        SerializedProperty AutoReturnTime;
        SerializedProperty isAllocated;
        SerializedProperty AvailableObjectList;
        SerializedProperty ActiveObjectList;

        bool ShowSettingsInEditor;
        bool ShowStaticEnumsInEditor;
        bool ShowDatasInEditor;
        void OnEnable()
        {
            EnumName = serializedObject.FindProperty("EnumName");
            EnumValue = serializedObject.FindProperty("EnumValue");
            EnumNameCorrect = serializedObject.FindProperty("EnumNameCorrect");
            SourceObject = serializedObject.FindProperty("SourceObject");
            SourceFilePath = serializedObject.FindProperty("SourceFilePath");
            PoolSize = serializedObject.FindProperty("PoolSize");
            AutoReturn = serializedObject.FindProperty("AutoReturn");
            AutoReturnTime = serializedObject.FindProperty("AutoReturnTime");
            isAllocated = serializedObject.FindProperty("isAllocated");
            AvailableObjectList = serializedObject.FindProperty("AvailableObjectList");
            ActiveObjectList = serializedObject.FindProperty("ActiveObjectList");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ObjectPool PL = target as ObjectPool;

            if (!EnumNameCorrect.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(EnumName, new GUIContent("Enum Name"));
                if (GUILayout.Button("Submit", GUILayout.Width(50)) || Event.current.keyCode == KeyCode.Return)
                {
                    Type type = EnumHelper.GetEnumType(EnumName.stringValue);
                    
                    if (type != null)
                    {
                        EnumNameCorrect.boolValue = true;
                        ShowSettingsInEditor = true;
                    }
                }
                EditorGUILayout.EndHorizontal();
            } else
            {
                if (!EditorApplication.isPlaying)
                {
                    if (GUILayout.Button("Settings")) ShowSettingsInEditor = !ShowSettingsInEditor;

                    if (ShowSettingsInEditor)
                    {
                        if (EnumNameCorrect.boolValue)
                        {
                            EnumValue.stringValue = EditorGUILayout.EnumPopup("PoolType", PL.PoolEnum).ToString();
                        } else
                        {
                            EditorGUILayout.LabelField("Can not get PoolEnum.");
                        }

                        if (!SourceObject.objectReferenceValue)
                            EditorGUILayout.PropertyField(SourceFilePath, new GUIContent("Source File Path"));
                        if (SourceFilePath.stringValue.Equals(string.Empty))
                            EditorGUILayout.PropertyField(SourceObject, new GUIContent("Source Prefab"));

                        EditorGUILayout.PropertyField(PoolSize, new GUIContent("PoolSize"));

                        EditorGUILayout.PropertyField(AutoReturn, new GUIContent("AutoReturn"));
                        if (AutoReturn.boolValue)
                        {
                            EditorGUILayout.Slider(AutoReturnTime, 0, 60, new GUIContent("Auto Return Time"));
                        }

                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Reset this pool")) 
                            CustomReset();
                        
                        GUI.enabled = SourceObject.objectReferenceValue || !SourceFilePath.stringValue.Equals(string.Empty);
                        if (!isAllocated.boolValue)
                        {
                            if (GUILayout.Button("Allocate", GUILayout.Width(EditorGUIUtility.currentViewWidth/2)))
                            {
                                if (!SourceObject.objectReferenceValue)
                                    SourceObject.objectReferenceValue = Resources.Load(SourceFilePath.stringValue) as GameObject;
                                Allocate();
                            }
                        } else if (GUILayout.Button("Destroy Objects", GUILayout.Width(EditorGUIUtility.currentViewWidth/2)))
                        {
                            ClearObjectPool();
                        }
                        GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space(20);
                    }
                } else
                {
                    ShowStaticEnumsInEditor = EditorGUILayout.Foldout(ShowStaticEnumsInEditor, new GUIContent("Static Included Enums", "Only show on playing"));

                    if (ShowStaticEnumsInEditor)
                    {
                        if (PL.IncludedEnumStringList.Count != ObjectPool.SPoolTupleDict.Count)
                        {
                            PL.IncludedEnumStringList.Clear();
                            PL.IncludedEnumCountList.Clear();
                            foreach(var v in ObjectPool.SPoolTupleDict)
                            {
                                PL.IncludedEnumStringList.Add(v.Key.GetType() + "." + v.Key.ToString());
                                PL.IncludedEnumCountList.Add(v.Value.Item2);
                            }
                        }

                        for (int i = 0; i < PL.IncludedEnumCountList.Count; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(i + ". " + PL.IncludedEnumStringList[i]);
                            EditorGUILayout.LabelField("Count : " + PL.IncludedEnumCountList[i], GUILayout.Width(100));
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }

                ShowDatasInEditor = EditorGUILayout.Foldout(ShowDatasInEditor, "Datas");

                if (ShowDatasInEditor)
                {

                    ShowAvilableObjects();
                    ShowActiveObjects();
                }
            }


            serializedObject.ApplyModifiedProperties();

            void Allocate()
            {
                ClearObjectPool();
                if (SourceObject.objectReferenceValue == null)
                {
                    Debug.LogError(PL.gameObject.name + " ObjectPool SourceObject is null. Check again");
                    return;
                }

                for (int i =0 ; i < PoolSize.intValue; i++)
                {
                    GameObject pooledObject = Instantiate(SourceObject.objectReferenceValue as GameObject, PL.transform);

                    pooledObject.name = pooledObject.name + i.ToString();
                    AvailableObjectList.InsertArrayElementAtIndex(AvailableObjectList.arraySize);
                    AvailableObjectList.GetArrayElementAtIndex(AvailableObjectList.arraySize-1).objectReferenceValue = pooledObject;
                    pooledObject.SetActive(false);
                }
                isAllocated.boolValue = true;
            }

            void CustomReset()
            {
                ClearObjectPool();
                EnumNameCorrect.boolValue = false;
                EnumName.stringValue = string.Empty;
                EnumValue.stringValue = string.Empty;
            }

            void ClearObjectPool()
            {
                for (int i = 0; i < AvailableObjectList.arraySize; i++)
                {
                    DestroyImmediate(AvailableObjectList.GetArrayElementAtIndex(i).objectReferenceValue);
                }

                for (int i = 0; i < ActiveObjectList.arraySize; i++)
                {
                    DestroyImmediate(ActiveObjectList.GetArrayElementAtIndex(i).objectReferenceValue);
                }

                AvailableObjectList.ClearArray();
                ActiveObjectList.ClearArray();

                isAllocated.boolValue = false;
            }

            void ShowAvilableObjects()
            {
                if (AvailableObjectList.arraySize == 0)
                    EditorGUILayout.LabelField("No Available Objects Now");                
                else
                    EditorGUILayout.LabelField("Available Objects");                

                for (int i = 0; i < AvailableObjectList.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Box(AvailableObjectList.GetArrayElementAtIndex(i).objectReferenceValue.name, GUILayout.Width(150));
                    EditorGUILayout.ObjectField(AvailableObjectList.GetArrayElementAtIndex(i).objectReferenceValue, typeof(GameObject), false);
                    EditorGUILayout.EndHorizontal();
                }
            }

            void ShowActiveObjects()
            {
                if (ActiveObjectList.arraySize == 0)
                    EditorGUILayout.LabelField("No Active Objects Now");
                else
                    EditorGUILayout.LabelField("Active Objects");
                    
                for (int i = 0; i < ActiveObjectList.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Box(ActiveObjectList.GetArrayElementAtIndex(i).objectReferenceValue.name, GUILayout.Width(150));
                    EditorGUILayout.ObjectField(ActiveObjectList.GetArrayElementAtIndex(i).objectReferenceValue, typeof(GameObject), false);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }
}