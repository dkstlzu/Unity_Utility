using UnityEngine;
using UnityEditor;

namespace dkstlzu.Utility
{
    [CustomEditor(typeof(DontDestroyManager))]
    public class DontDestroyManagerEditor : Editor
    {
        SerializedProperty _targetComponent;
        SerializedProperty _useUniqueComponent;
        SerializedProperty _uniqueComponent;

        void OnEnable()
        {
            _targetComponent = serializedObject.FindProperty(nameof(DontDestroyManager.TargetComponent));
            _useUniqueComponent = serializedObject.FindProperty(nameof(DontDestroyManager.UseUniqueComponent));
            _uniqueComponent = serializedObject.FindProperty(nameof(DontDestroyManager.UniqueComponent));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (!_useUniqueComponent.boolValue)
            {
                if (GUILayout.Button("Use UniqueComponent"))
                {
                    DontDestroyManager targetDDM = (DontDestroyManager)target;
                    _uniqueComponent.objectReferenceValue = targetDDM.gameObject.AddComponent(typeof(UniqueComponent));
                    _useUniqueComponent.boolValue = true;
                }
            } else
            {
                if (_uniqueComponent.objectReferenceValue == null)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("UniqueComponent reference corrupted check again or remake component");
                    if (GUILayout.Button("Reset"))
                    {
                        _useUniqueComponent.boolValue = false;
                    }
                    GUILayout.EndHorizontal();
                } else if (GUILayout.Button("Dont use UniqueComponent"))
                {
                    _useUniqueComponent.boolValue = false;
                    DestroyImmediate(_uniqueComponent.objectReferenceValue);
                }
            }
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_targetComponent);
            if (EditorGUI.EndChangeCheck())
            {
                if (_useUniqueComponent.boolValue)
                {
                    ((UniqueComponent)_uniqueComponent.objectReferenceValue).TargetComponent = (Component)_targetComponent.objectReferenceValue;
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}