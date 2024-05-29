using UnityEngine;
using UnityEditor;

namespace dkstlzu.Utility
{
    [CustomEditor(typeof(EventTrigger), true)]
    [CanEditMultipleObjects]
    public class EventTriggerEditor : Editor
    {
        protected SerializedProperty IsReady;
        protected SerializedProperty Collider;
        protected SerializedProperty Collider2D;
        protected SerializedProperty Use2D;
        protected SerializedProperty TargetLayerMask;
        protected SerializedProperty OnTriggerEnterEvent;
        protected SerializedProperty OnTriggerStayEvent;
        protected SerializedProperty OnTriggerExitEvent;
        protected SerializedProperty PlayOnlyFirst;

        protected virtual void OnEnable()
        {
            IsReady = serializedObject.FindProperty(nameof(EventTrigger.IsReady));
            Collider = serializedObject.FindProperty(nameof(EventTrigger.Collider));
            Collider2D = serializedObject.FindProperty(nameof(EventTrigger.Collider2D));
            Use2D = serializedObject.FindProperty(nameof(EventTrigger.Use2D));
            TargetLayerMask = serializedObject.FindProperty(nameof(EventTrigger.TargetLayerMask));
            OnTriggerEnterEvent = serializedObject.FindProperty(nameof(EventTrigger.OnTriggerEnterEvent));
            OnTriggerStayEvent = serializedObject.FindProperty(nameof(EventTrigger.OnTriggerStayEvent));
            OnTriggerExitEvent = serializedObject.FindProperty(nameof(EventTrigger.OnTriggerExitEvent));
            PlayOnlyFirst = serializedObject.FindProperty(nameof(EventTrigger.PlayOnlyFirst));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EventTrigger ET = (EventTrigger)target;

            if (!IsReady.boolValue)
            {
                EditorGUILayout.LabelField("Select your target Collider type.");

                // 3D Colliders
                if (GUILayout.Button("Box Collider"))
                {
                    Collider.objectReferenceValue = ET.gameObject.AddComponent<BoxCollider>();
                    Set3DColliderSettings();
                }
                if (GUILayout.Button("Capsule Collider"))
                {
                    Collider.objectReferenceValue = ET.gameObject.AddComponent<CapsuleCollider>();
                    Set3DColliderSettings();
                }
                if (GUILayout.Button("Sphere Collider"))
                {
                    Collider.objectReferenceValue = ET.gameObject.AddComponent<SphereCollider>();
                    Set3DColliderSettings();
                }
                if (GUILayout.Button("Mesh Collider"))
                {
                    Collider.objectReferenceValue = ET.gameObject.AddComponent<MeshCollider>();
                    Set3DColliderSettings();
                }

                void Set3DColliderSettings()
                {
                    IsReady.boolValue = true;
                    Use2D.boolValue = false;
                    ((Collider)Collider.objectReferenceValue).isTrigger = true;
                    UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(ET.Collider, false);
                }

                // 2D Colliders
                if (GUILayout.Button("Box2D Collider"))
                {
                    Collider2D.objectReferenceValue = ET.gameObject.AddComponent<BoxCollider2D>();
                    Set2DColliderSettings();
                }
                if (GUILayout.Button("Capsule2D Collider"))
                {
                    Collider2D.objectReferenceValue = ET.gameObject.AddComponent<CapsuleCollider2D>();
                    Set2DColliderSettings();
                }
                if (GUILayout.Button("Circle2D Collider"))
                {
                    Collider2D.objectReferenceValue = ET.gameObject.AddComponent<CircleCollider2D>();
                    Set2DColliderSettings();
                }
                if (GUILayout.Button("Polygon2D Collider"))
                {
                    Collider2D.objectReferenceValue = ET.gameObject.AddComponent<PolygonCollider2D>();
                    Set2DColliderSettings();
                }
                if (GUILayout.Button("Composite2D Collider"))
                {
                    Collider2D.objectReferenceValue = ET.gameObject.AddComponent<CompositeCollider2D>();
                    Set2DColliderSettings();
                }
                if (GUILayout.Button("Edge2D Collider"))
                {
                    Collider2D.objectReferenceValue = ET.gameObject.AddComponent<EdgeCollider2D>();
                    Set2DColliderSettings();
                }

                void Set2DColliderSettings()
                {
                    IsReady.boolValue = true;
                    Use2D.boolValue = true;
                    ((Collider2D)Collider2D.objectReferenceValue).isTrigger = true;
                    UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(ET.Collider2D, false);
                }
            } else
            {
                if (GUILayout.Button("Change Collider Type"))
                {
                    if (ET.Collider != null)
                    {
                        DestroyImmediate(ET.Collider);
                    }
                    if (ET.Collider2D != null)
                    {
                        DestroyImmediate(ET.Collider2D);
                    }
                    IsReady.boolValue = false;
                }

                if (Use2D.boolValue && !Collider2D.objectReferenceValue && IsReady.boolValue || !Use2D.boolValue && !Collider.objectReferenceValue && IsReady.boolValue) IsReady.boolValue = false;
                
                EditorGUILayout.PropertyField(TargetLayerMask);
                EditorGUILayout.PropertyField(OnTriggerEnterEvent);
                EditorGUILayout.PropertyField(OnTriggerStayEvent);
                EditorGUILayout.PropertyField(OnTriggerExitEvent);
                EditorGUILayout.PropertyField(PlayOnlyFirst);
            }         
            serializedObject.ApplyModifiedProperties();
        }
    }
}