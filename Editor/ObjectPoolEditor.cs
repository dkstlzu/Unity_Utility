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
            SerializedProperty property = new SerializedObject(Selection.activeGameObject.GetComponent<ObjectPool>()).GetIterator();

            string PropString = "";

            while(property.Next(true))
            {
                PropString += property.propertyPath + "\n";
            }

            MonoBehaviour.print("Properties of this ObjectPool");
            MonoBehaviour.print(PropString);
        }

        SerializedProperty EnumName;
        SerializedProperty EnumValue;
        SerializedProperty EnumNameCorrect;
        SerializedProperty SourceObject;
        SerializedProperty SourceFilePath;
        SerializedProperty PoolSize;
        SerializedProperty AutoReturn;
        SerializedProperty AutoReturnTime;

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
                        if (PL.PoolEnum != null)
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
                        if (!PL.isAllocated)
                        {
                            if (GUILayout.Button("Allocate", GUILayout.Width(EditorGUIUtility.currentViewWidth/2)))
                            {
                                if (!SourceObject.objectReferenceValue)
                                    SourceObject.objectReferenceValue = Resources.Load(SourceFilePath.stringValue) as GameObject;
                                PL.Allocate();
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

            void CustomReset()
            {
                var DictRef = ObjectPool.SPoolTupleDict;
                if (DictRef.ContainsKey(PL.PoolEnum))
                {
                    if (DictRef[PL.PoolEnum].Item2 <= 1)
                        DictRef.Remove(PL.PoolEnum);
                    else
                        DictRef[PL.PoolEnum] = (DictRef[PL.PoolEnum].Item1, DictRef[PL.PoolEnum].Item2 - 1);

                    ClearObjectPool();
                }
                PL.gameObject.AddComponent<ObjectPool>();
                DestroyImmediate(PL);
            }

            void ClearObjectPool()
            {
                PL.Clear();
            }

            void ShowAvilableObjects()
            {
                if (PL.AvailableObjects.Length == 0)
                    EditorGUILayout.LabelField("No Available Objects Now");                
                else
                    EditorGUILayout.LabelField("Available Objects");                

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
                if (PL.ActiveObjects.Length == 0)
                    EditorGUILayout.LabelField("No Active Objects Now");
                else
                    EditorGUILayout.LabelField("Active Objects");
                    
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