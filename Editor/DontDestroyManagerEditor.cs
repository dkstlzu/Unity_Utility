using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Utility
{
    [CustomEditor(typeof(DontDestroyManager))]
    public class DontDestroyManagerEditor : Editor
    {

        SerializedProperty Uniqueness;
        SerializedProperty TargetComponent;
        SerializedProperty ReplaceAsNew;
        SerializedProperty HashID;

        void OnEnable()
        {
            Uniqueness = serializedObject.FindProperty("Uniqueness");
            TargetComponent = serializedObject.FindProperty("TargetComponent");
            ReplaceAsNew = serializedObject.FindProperty("ReplaceAsNew");
            HashID = serializedObject.FindProperty("HashID");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DontDestroyManager DDM = target as DontDestroyManager;

            EditorGUILayout.PropertyField(Uniqueness, new GUIContent("Method"));
            EditorGUILayout.PropertyField(TargetComponent, new GUIContent("Target Component"));
            EditorGUILayout.PropertyField(ReplaceAsNew, new GUIContent("Replace as new when duplicated"));

            if (Uniqueness.enumValueIndex == EnumHelper.GetIndexOf(DontDestroyManager.DontDestroyMethod.UniqueID))
            {
                EditorGUILayout.PropertyField(HashID, new GUIContent("Unique ID"));
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}