using UnityEngine;
using UnityEditor;

namespace Utility
{
    public static class EditorInspectorUtility
    {
        public static void GroupPropertyField(ref bool status, string header, SerializedProperty[] properties)
        {
            status = EditorGUILayout.BeginFoldoutHeaderGroup(status, header);
            if (status)
            foreach(SerializedProperty property in properties)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.PropertyField(property);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}