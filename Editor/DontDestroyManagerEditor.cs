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
        SerializedProperty ReplacePreviousOne;
        SerializedProperty HashID;
        SerializedProperty DestroyGO;

        void OnEnable()
        {
            Uniqueness = serializedObject.FindProperty("Uniqueness");
            TargetComponent = serializedObject.FindProperty("TargetComponent");
            ReplacePreviousOne = serializedObject.FindProperty("ReplacePreviousOne");
            HashID = serializedObject.FindProperty("HashID");
            DestroyGO = serializedObject.FindProperty("DestroyGameObject");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DontDestroyManager DDM = target as DontDestroyManager;

            EditorGUILayout.PropertyField(Uniqueness, new GUIContent("Method"));
            EditorGUILayout.PropertyField(TargetComponent, new GUIContent("Target Component"));
            EditorGUILayout.PropertyField(ReplacePreviousOne, new GUIContent("Replace as new when duplicated"));
            EditorGUILayout.PropertyField(DestroyGO, new GUIContent("GO will be Destroy"));

            if (Uniqueness.enumValueIndex == EnumHelper.GetIndexOf(DontDestroyManager.DontDestroyMethod.UniqueID))
            {
                EditorGUILayout.PropertyField(HashID, new GUIContent("Unique ID"));
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}