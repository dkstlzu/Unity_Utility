using UnityEngine;
using UnityEditor;

namespace dkstlzu.Utility
{
    [CustomEditor(typeof(EventTrigger), true)]
    [CanEditMultipleObjects]
    public class EventTriggerEditor : Editor
    {
        protected SerializedProperty isReady;
        protected SerializedProperty Collider;
        protected SerializedProperty Collider2D;
        protected SerializedProperty use2D;
        protected SerializedProperty DrawGizmo;
        protected SerializedProperty TargetLayerMask;
        protected SerializedProperty OnTriggerEnterEvent;
        protected SerializedProperty OnTriggerStayEvent;
        protected SerializedProperty OnTriggerExitEvent;
        protected SerializedProperty PlayOnlyFirst;

        protected virtual void OnEnable()
        {
            isReady = serializedObject.FindProperty("isReady");
            Collider = serializedObject.FindProperty("Collider");
            Collider2D = serializedObject.FindProperty("Collider2D");
            use2D = serializedObject.FindProperty("use2D");
            DrawGizmo = serializedObject.FindProperty("DrawGizmo");
            TargetLayerMask = serializedObject.FindProperty("TargetLayerMask");
            OnTriggerEnterEvent = serializedObject.FindProperty("OnTriggerEnterEvent");
            OnTriggerStayEvent = serializedObject.FindProperty("OnTriggerStayEvent");
            OnTriggerExitEvent = serializedObject.FindProperty("OnTriggerExitEvent");
            PlayOnlyFirst = serializedObject.FindProperty("PlayOnlyFirst");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EventTrigger ET = (EventTrigger)target;

            if (!isReady.boolValue)
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
                    isReady.boolValue = true;
                    use2D.boolValue = false;
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
                    isReady.boolValue = true;
                    use2D.boolValue = true;
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
                    isReady.boolValue = false;
                }

                if (use2D.boolValue && !Collider2D.objectReferenceValue && isReady.boolValue || !use2D.boolValue && !Collider.objectReferenceValue && isReady.boolValue) isReady.boolValue = false;
                
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