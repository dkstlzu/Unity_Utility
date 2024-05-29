using UnityEngine;
using UnityEditor;

namespace dkstlzu.Utility
{
    [CustomEditor(typeof(UniqueComponent))]
    public class UniqueComponentEditor : Editor
    {
        SerializedProperty _uniqueness;
        SerializedProperty _targetComponent;
        SerializedProperty _replacePreviousOne;
        SerializedProperty _hashID;

        void OnEnable()
        {
            _targetComponent = serializedObject.FindProperty(nameof(UniqueComponent.TargetComponent));
            _uniqueness = serializedObject.FindProperty(nameof(UniqueComponent.Uniqueness));
            _replacePreviousOne = serializedObject.FindProperty(nameof(UniqueComponent.ReplacePreviousOne));
            _hashID = serializedObject.FindProperty(nameof(UniqueComponent.HashID));
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_uniqueness, new GUIContent("Method"));
            EditorGUILayout.PropertyField(_targetComponent, new GUIContent("Target Component"));
            EditorGUILayout.PropertyField(_replacePreviousOne, new GUIContent("Replace as new when duplicated"));

            if (_uniqueness.enumValueIndex == EnumHelper.GetIndexOf(UniqueComponent.UniqueComponentMethod.UniqueID))
            {
                EditorGUILayout.PropertyField(_hashID, new GUIContent("Unique ID"));
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}