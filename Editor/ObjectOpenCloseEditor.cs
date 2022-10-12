using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace  dkstlzu.Utility
{
    [CustomEditor(typeof(ObjectOpenClose))]
    public class ObjectOpenCloseEditor : Editor
    {
        SerializedObject targetTranstormSO;

        SerializedProperty isOpened;
        SerializedProperty localScale;
        
        void OnEnable()
        {
            isOpened = serializedObject.FindProperty("isOpened");

            ObjectOpenClose targetOOC = target as ObjectOpenClose;

            if (targetOOC.Target)
            {
                targetTranstormSO = new SerializedObject(targetOOC.Target);
                localScale = targetTranstormSO.FindProperty("m_LocalScale");
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            serializedObject.Update();
            targetTranstormSO.Update();

            if (localScale == null) return;
            if (isOpened.boolValue)
            {
                if (GUILayout.Button("Close"))
                {
                    isOpened.boolValue = false;
                    localScale.FindPropertyRelative("x").floatValue = 0;
                    localScale.FindPropertyRelative("y").floatValue = 0;
                }
            } else
            {
                if (GUILayout.Button("Open"))
                {
                    isOpened.boolValue = true;
                    localScale.FindPropertyRelative("x").floatValue = 1;
                    localScale.FindPropertyRelative("y").floatValue = 1;
                }
            }

            serializedObject.ApplyModifiedProperties();
            targetTranstormSO.ApplyModifiedProperties();
        }
    }
}