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
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ObjectPool PL = target as ObjectPool;

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
                        PL.ShowSettingsInEditor = true;
                    }
                }
                EditorGUILayout.EndHorizontal();
            } else
            {
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
                            ClearObjectPool();
                        }
                        GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space(20);
                    }
                } else
                {
                    PL.ShowStaticEnumsInEditor = EditorGUILayout.BeginFoldoutHeaderGroup(PL.ShowStaticEnumsInEditor, new GUIContent("Static Included Enums", "Only show on playing"));
                    EditorGUILayout.EndFoldoutHeaderGroup();

                    if (PL.ShowStaticEnumsInEditor)
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
                if (ObjectPool.SPoolTupleDict.ContainsKey(PL.PoolEnum))
                {
                    if (ObjectPool.SPoolTupleDict[PL.PoolEnum].Item2 <= 1)
                        ObjectPool.SPoolTupleDict.Remove(PL.PoolEnum);
                    else
                        ObjectPool.SPoolTupleDict[PL.PoolEnum] = (ObjectPool.SPoolTupleDict[PL.PoolEnum].Item1, ObjectPool.SPoolTupleDict[PL.PoolEnum].Item2 - 1);

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