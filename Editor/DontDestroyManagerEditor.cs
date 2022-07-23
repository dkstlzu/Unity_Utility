using UnityEngine;
using UnityEditor;

namespace Utility
{
    [CustomEditor(typeof(DontDestroyManager))]
    public class DontDestroyManagerEditor : Editor
    {
        SerializedProperty TargetComponent;
        SerializedProperty useUniqueComponent;
        SerializedProperty UniqueComponent;

        void OnEnable()
        {
            TargetComponent = serializedObject.FindProperty("TargetComponent");
            useUniqueComponent = serializedObject.FindProperty("useUniqueComponent");
            UniqueComponent = serializedObject.FindProperty("UniqueComponent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (!useUniqueComponent.boolValue)
            {
                if (GUILayout.Button("Use UniqueComponent"))
                {
                    DontDestroyManager targetDDM = (DontDestroyManager)target;
                    UniqueComponent.objectReferenceValue = targetDDM.gameObject.AddComponent(typeof(UniqueComponent));
                    useUniqueComponent.boolValue = true;
                }
            } else
            {
                if (UniqueComponent.objectReferenceValue == null)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("UniqueComponent reference corrupted check again or remake component");
                    if (GUILayout.Button("Reset"))
                    {
                        useUniqueComponent.boolValue = false;
                    }
                    GUILayout.EndHorizontal();
                } else if (GUILayout.Button("Dont use UniqueComponent"))
                {
                    useUniqueComponent.boolValue = false;
                    DestroyImmediate(UniqueComponent.objectReferenceValue);
                }
            }
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(TargetComponent);
            if (EditorGUI.EndChangeCheck())
            {
                ((UniqueComponent)UniqueComponent.objectReferenceValue).TargetComponent = (Component)TargetComponent.objectReferenceValue;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}