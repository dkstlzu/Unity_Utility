using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Utility
{
    [CustomEditor(typeof(EnumSettableMonoBehaviour))]
    public class EnumSettableEditor : Editor
    {
        SerializedProperty EnumName;
        SerializedProperty EnumValue;
        SerializedProperty EnumNameCorrect;

        bool UseEnumValuePopUp;

        protected virtual EnumSettableMonoBehaviour GetTarget()
        {
            return target as EnumSettableMonoBehaviour;
        }

        protected virtual void OnEnable()
        {
            EnumName = serializedObject.FindProperty("enumName");
            EnumValue = serializedObject.FindProperty("enumValue");
            EnumNameCorrect = serializedObject.FindProperty("enumNameCorrect");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EnumSettableMonoBehaviour ESM = GetTarget();

            if (!EnumNameCorrect.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(EnumName, new GUIContent("Enum Name"));
                if (GUILayout.Button("Submit", GUILayout.Width(50)))
                {
                    Type type = EnumHelper.GetEnumType(EnumName.stringValue);
                    
                    if (type != null)
                    {
                        EnumNameCorrect.boolValue = true;
                    }
                }
                EditorGUILayout.EndHorizontal();
            } else
            {
                if (!EditorApplication.isPlaying)
                {
                    if (GUILayout.Button($"{EnumName.stringValue} : Reset enum type")) 
                        EnumReset();

                    UseEnumValuePopUp = EditorGUILayout.Toggle("UseEnumValuePopUp", UseEnumValuePopUp);
                    if (UseEnumValuePopUp)
                    {
                        if (EnumNameCorrect.boolValue)
                        {
                            EnumValue.stringValue = EditorGUILayout.EnumPopup("Enum Value", ESM.EnumValue).ToString();
                        } else
                        {
                            EditorGUILayout.LabelField("Can not get PoolEnum.");
                        }
                    } 
                    OnNotPlayingInspectorGUI();
                } else
                {
                    OnPlayingInspectorGUI();
                }

                OnOverridingInspectorGUI();
            }
            serializedObject.ApplyModifiedProperties();
        }
        protected void EnumReset()
        {
            EnumNameCorrect.boolValue = false;
            EnumName.stringValue = string.Empty;
            EnumValue.stringValue = string.Empty;
        }

        protected virtual void OnPlayingInspectorGUI() {}
        protected virtual void OnNotPlayingInspectorGUI() {}
        protected virtual void OnOverridingInspectorGUI() {}
    }
}