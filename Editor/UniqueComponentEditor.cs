using UnityEngine;
using UnityEditor;

namespace dkstlzu.Utility
{
    [CustomEditor(typeof(UniqueComponent))]
    public class UniqueComponentEditor : Editor
    {
        SerializedProperty Uniqueness;
        SerializedProperty TargetComponent;
        SerializedProperty ReplacePreviousOne;
        SerializedProperty HashID;
        SerializedProperty DestroyGO;
        SerializedProperty DestroyRequiredComponents;

        void OnEnable()
        {
            TargetComponent = serializedObject.FindProperty("TargetComponent");
            Uniqueness = serializedObject.FindProperty("Uniqueness");
            ReplacePreviousOne = serializedObject.FindProperty("ReplacePreviousOne");
            HashID = serializedObject.FindProperty("HashID");
            DestroyGO = serializedObject.FindProperty("DestroyGameObject");
            DestroyRequiredComponents = serializedObject.FindProperty("DestroyRequiredComponentsAlso");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(Uniqueness, new GUIContent("Method"));
            EditorGUILayout.PropertyField(TargetComponent, new GUIContent("Target Component"));
            EditorGUILayout.PropertyField(ReplacePreviousOne, new GUIContent("Replace as new when duplicated"));
            EditorGUILayout.PropertyField(DestroyGO, new GUIContent("GO will be Destroy"));

            if (Uniqueness.enumValueIndex == EnumHelper.GetIndexOf(UniqueComponent.UniqueComponentMethod.UniqueID))
            {
                EditorGUILayout.PropertyField(HashID, new GUIContent("Unique ID"));
            }
            EditorGUILayout.PropertyField(DestroyRequiredComponents);
            serializedObject.ApplyModifiedProperties();
        }
    }
}