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
        public override void OnInspectorGUI()
        {
            DontDestroyManager DDM = target as DontDestroyManager;

            DDM.Uniqueness = (DontDestroyManager.DontDestroyMethod)EditorGUILayout.EnumPopup("Method", DDM.Uniqueness);
            DDM.TargetComponent = (Component)EditorGUILayout.ObjectField("Target Component",DDM.TargetComponent, typeof(Component), true);

            // EditorGUILayout.BeginHorizontal();
            // EditorGUILayout.LabelField("Replace as new when duplicated");
            // GUILayout.FlexibleSpace();
            DDM.ReplaceAsNew = EditorGUILayout.ToggleLeft("Replace as new when duplicated", DDM.ReplaceAsNew);
            // EditorGUILayout.EndHorizontal();

            if (DDM.Uniqueness == DontDestroyManager.DontDestroyMethod.UniqueID)
            {
                DDM.HashID = EditorGUILayout.TextField("Unique ID",DDM.HashID);
            }
        }
    }
}