using System;
using UnityEngine;
using UnityEditor;

namespace Utility
{
    [CustomEditor(typeof(SoundArgs))]
    public class SoundArgsEditor : Editor
    {
        SerializedProperty EnumName;
        SerializedProperty EnumValue;
        SerializedProperty EnumNameCorrect;
        SerializedProperty SoundPlayMode;
        SerializedProperty Transform;
        SerializedProperty RelativePosition;
        SerializedProperty AutoReturn;
        SoundArgs SA;
        void OnEnable()
        {
            EnumName = serializedObject.FindProperty("EnumName");
            EnumValue = serializedObject.FindProperty("EnumValue");
            EnumNameCorrect = serializedObject.FindProperty("EnumNameCorrect");
            SoundPlayMode = serializedObject.FindProperty("SoundPlayMode");
            Transform = serializedObject.FindProperty("Transform");
            RelativePosition = serializedObject.FindProperty("RelativePosition");
            AutoReturn = serializedObject.FindProperty("AutoReturn");
            SA = (SoundArgs)target;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (EnumNameCorrect.boolValue)
            {
                EnumValue.stringValue = EditorGUILayout.EnumPopup("SoundNaming", SA.SoundNaming).ToString();
            } else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(EnumName, new GUIContent("Enum Name"));
                if (GUILayout.Button("Submit", GUILayout.Width(50)) || Event.current.keyCode == KeyCode.Return)
                {
                    Type type = EnumHelper.GetEnumType(EnumName.stringValue);
                    
                    if (type != null)
                    {
                        EnumNameCorrect.boolValue = true;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.PropertyField(SoundPlayMode, new GUIContent("Sound Play Mode"));
            EditorGUILayout.PropertyField(Transform, new GUIContent("Transform"));
            EditorGUILayout.PropertyField(RelativePosition, new GUIContent("Relative Position"));
            EditorGUILayout.PropertyField(AutoReturn, new GUIContent("Auto Return"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}