using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace dkstlzu.Utility
{
    [CustomEditor(typeof(EnumSettableMonoBehaviour), true)]
    public class EnumSettableEditor : Editor
    {
        SerializedProperty EnumName;
        SerializedProperty EnumValue;
        SerializedProperty EnumNameCorrect;

        private EnumSettableMonoBehaviour GetTarget()
        {
            return target as EnumSettableMonoBehaviour;
        }

        protected virtual void OnEnable()
        {
            EnumName = serializedObject.FindProperty(nameof(EnumSettableMonoBehaviour.EnumName));
            EnumValue = serializedObject.FindProperty(nameof(EnumSettableMonoBehaviour._EnumValue));
            EnumNameCorrect = serializedObject.FindProperty(nameof(EnumSettableMonoBehaviour.EnumNameCorrect));
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

                    OnNotPlayingInspectorGUI();
                } else
                {
                    OnPlayingInspectorGUI();
                }

                OnOverridingInspectorGUI();
            }
            
            base.OnInspectorGUI();
            serializedObject.ApplyModifiedProperties();
        }

        protected void EnumPopup()
        {
            if (EnumNameCorrect.boolValue)
            {
                EnumValue.stringValue = EditorGUILayout.EnumPopup("Enum Value", GetTarget().EnumValue).ToString();
            } else
            {
                EditorGUILayout.LabelField("Can not get PoolEnum.");
            }
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