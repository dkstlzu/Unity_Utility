using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace  dkstlzu.Utility.UI
{
    [CustomEditor(typeof(ObjectOpenClose))]
    public class ObjectOpenCloseEditor : Editor
    {
        SerializedObject _targetTranstormSO;

        SerializedProperty _isOpened;
        SerializedProperty _localScale;
        
        void OnEnable()
        {
            _isOpened = serializedObject.FindProperty(nameof(ObjectOpenClose.IsOpened));

            ObjectOpenClose targetOOC = target as ObjectOpenClose;

            if (targetOOC.Target)
            {
                _targetTranstormSO = new SerializedObject(targetOOC.Target);
                _localScale = _targetTranstormSO.FindProperty("m_LocalScale");
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            serializedObject.Update();
            _targetTranstormSO.Update();

            if (_localScale == null) return;
            if (_isOpened.boolValue)
            {
                if (GUILayout.Button("Close"))
                {
                    _isOpened.boolValue = false;
                    _localScale.FindPropertyRelative("x").floatValue = 0;
                    _localScale.FindPropertyRelative("y").floatValue = 0;
                }
            } else
            {
                if (GUILayout.Button("Open"))
                {
                    _isOpened.boolValue = true;
                    _localScale.FindPropertyRelative("x").floatValue = 1;
                    _localScale.FindPropertyRelative("y").floatValue = 1;
                }
            }

            serializedObject.ApplyModifiedProperties();
            _targetTranstormSO.ApplyModifiedProperties();
        }
    }
}