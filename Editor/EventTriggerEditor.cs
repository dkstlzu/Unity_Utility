using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Utility
{
    [CustomEditor(typeof(EventTrigger))]
    public class EventTriggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EventTrigger ET = (EventTrigger)target;

            if (!ET.isReady)
            {
                EditorGUILayout.LabelField("Select your target Collider type.");

                // 3D Colliders
                if (GUILayout.Button("Box Collider"))
                {
                    ET.Collider = ET.gameObject.AddComponent<BoxCollider>();
                    Set3DColliderSettings();
                }
                if (GUILayout.Button("Capsule Collider"))
                {
                    ET.Collider = ET.gameObject.AddComponent<CapsuleCollider>();
                    Set3DColliderSettings();
                }
                if (GUILayout.Button("Sphere Collider"))
                {
                    ET.Collider = ET.gameObject.AddComponent<SphereCollider>();
                    Set3DColliderSettings();
                }
                if (GUILayout.Button("Mesh Collider"))
                {
                    ET.Collider = ET.gameObject.AddComponent<MeshCollider>();
                    Set3DColliderSettings();
                }

                void Set3DColliderSettings()
                {
                    ET.isReady = true;
                    ET.use2D = false;
                    ET.Collider.isTrigger = true;
                    UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(ET.Collider, false);
                }

                // 2D Colliders
                if (GUILayout.Button("Box2D Collider"))
                {
                    ET.Collider2D = ET.gameObject.AddComponent<BoxCollider2D>();
                    Set2DColliderSettings();
                }
                if (GUILayout.Button("Capsule2D Collider"))
                {
                    ET.Collider2D = ET.gameObject.AddComponent<CapsuleCollider2D>();
                    Set2DColliderSettings();
                }
                if (GUILayout.Button("Circle2D Collider"))
                {
                    ET.Collider2D = ET.gameObject.AddComponent<CircleCollider2D>();
                    Set2DColliderSettings();
                }
                if (GUILayout.Button("Polygon2D Collider"))
                {
                    ET.Collider2D = ET.gameObject.AddComponent<PolygonCollider2D>();
                    Set2DColliderSettings();
                }
                if (GUILayout.Button("Composite2D Collider"))
                {
                    ET.Collider2D = ET.gameObject.AddComponent<CompositeCollider2D>();
                    Set2DColliderSettings();
                }
                if (GUILayout.Button("Edge2D Collider"))
                {
                    ET.Collider2D = ET.gameObject.AddComponent<EdgeCollider2D>();
                    Set2DColliderSettings();
                }

                void Set2DColliderSettings()
                {
                    ET.isReady = true;
                    ET.use2D = true;
                    ET.Collider2D.isTrigger = true;
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
                    ET.isReady = false;
                }

                if (ET.use2D && !ET.Collider2D && ET.isReady || !ET.use2D && !ET.Collider && ET.isReady) ET.isReady = false;
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("TargetLayerMask"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnTriggerEnterEvent"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnTriggerStayEvent"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnTriggerExitEvent"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PlayOnlyFirst"));
            }         
            serializedObject.ApplyModifiedProperties();
        }
    }
}