using System;
using UnityEngine;
using UnityEditor;
using Debug = System.Diagnostics.Debug;
using Object = UnityEngine.Object;

namespace dkstlzu.Utility
{
    public class ColliderEditor : Editor
    {
        protected SerializedProperty IsReady;
        protected SerializedProperty Collider;

        protected string IsReadyName = "IsReady";
        protected string ColliderPropertyName = "_collider";
        
        protected virtual void OnEnable()
        {
            IsReady = serializedObject.FindProperty(IsReadyName);
            Collider = serializedObject.FindProperty(ColliderPropertyName);
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Component component = target as Component;
            Debug.Assert(component != null, "Target is not component");

            if (!IsReady.boolValue)
            {
                EditorGUILayout.LabelField("Select your target Collider type.");

                // 3D Colliders
                if (GUILayout.Button("Box Collider"))
                {
                    Collider.objectReferenceValue = component.gameObject.AddComponent<BoxCollider>();
                    Set3DColliderSettings();
                }
                if (GUILayout.Button("Capsule Collider"))
                {
                    Collider.objectReferenceValue = component.gameObject.AddComponent<CapsuleCollider>();
                    Set3DColliderSettings();
                }
                if (GUILayout.Button("Sphere Collider"))
                {
                    Collider.objectReferenceValue = component.gameObject.AddComponent<SphereCollider>();
                    Set3DColliderSettings();
                }
                if (GUILayout.Button("Mesh Collider"))
                {
                    Collider.objectReferenceValue = component.gameObject.AddComponent<MeshCollider>();
                    Set3DColliderSettings();
                }

                // 2D Colliders
                if (GUILayout.Button("Box2D Collider"))
                {
                    Collider.objectReferenceValue = component.gameObject.AddComponent<BoxCollider2D>();
                    Set2DColliderSettings();
                }
                if (GUILayout.Button("Capsule2D Collider"))
                {
                    Collider.objectReferenceValue = component.gameObject.AddComponent<CapsuleCollider2D>();
                    Set2DColliderSettings();
                }
                if (GUILayout.Button("Circle2D Collider"))
                {
                    Collider.objectReferenceValue = component.gameObject.AddComponent<CircleCollider2D>();
                    Set2DColliderSettings();
                }
                if (GUILayout.Button("Polygon2D Collider"))
                {
                    Collider.objectReferenceValue = component.gameObject.AddComponent<PolygonCollider2D>();
                    Set2DColliderSettings();
                }
                if (GUILayout.Button("Composite2D Collider"))
                {
                    Collider.objectReferenceValue = component.gameObject.AddComponent<CompositeCollider2D>();
                    Set2DColliderSettings();
                }
                if (GUILayout.Button("Edge2D Collider"))
                {
                    Collider.objectReferenceValue = component.gameObject.AddComponent<EdgeCollider2D>();
                    Set2DColliderSettings();
                }
                
                OverrideNotReady();
            } else
            {
                if (!Collider.objectReferenceValue) IsReady.boolValue = false;

                if (GUILayout.Button("Change Collider Type"))
                {
                    ChangeColliderType();
                }
                
                OverrideOnReady();
            }         
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OverrideNotReady()
        {
            
        }

        protected virtual void OverrideOnReady()
        {
            
        }

        protected virtual void ChangeColliderType()
        {
            DestroyImmediate(Collider.objectReferenceValue);
                    
            IsReady.boolValue = false;
        }
        
        protected virtual void Set3DColliderSettings()
        {
            IsReady.boolValue = true;
            ((Collider)Collider.objectReferenceValue).isTrigger = true;
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(Collider.objectReferenceValue, false);
        }
        
        protected virtual void Set2DColliderSettings()
        {
            IsReady.boolValue = true;
            ((Collider2D)Collider.objectReferenceValue).isTrigger = true;
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(Collider.objectReferenceValue, false);
        }
    }
    
    [CustomEditor(typeof(EventTrigger), true)]
    [CanEditMultipleObjects]
    public class EventTriggerEditor : ColliderEditor
    {
        protected SerializedProperty Rigidbody;
        protected SerializedProperty TargetTags;
        protected SerializedProperty TargetLayerMask;
        protected SerializedProperty OnTriggerEnterEvent;
        protected SerializedProperty OnTriggerEnterGOEvent;
        protected SerializedProperty OnTriggerStayEvent;
        protected SerializedProperty OnTriggerStayGOEvent;
        protected SerializedProperty OnTriggerExitEvent;
        protected SerializedProperty OnTriggerExitGOEvent;
        protected SerializedProperty PlayOnlyFirst;

        protected EventTrigger _et;

        private string[] _tags;
        private int _tagMask;
        
        protected override void OnEnable()
        {
            ColliderPropertyName = "_collider";
            
            base.OnEnable();
            
            Rigidbody = serializedObject.FindProperty(nameof(EventTrigger.Rigidbody));
            TargetTags = serializedObject.FindProperty(nameof(EventTrigger.TargetTags));
            TargetLayerMask = serializedObject.FindProperty(nameof(EventTrigger.TargetLayerMask));
            OnTriggerEnterEvent = serializedObject.FindProperty(nameof(EventTrigger.OnTriggerEnterEvent));
            OnTriggerEnterGOEvent = serializedObject.FindProperty(nameof(EventTrigger.OnTriggerEnterGOEvent));
            OnTriggerStayEvent = serializedObject.FindProperty(nameof(EventTrigger.OnTriggerStayEvent));
            OnTriggerStayGOEvent = serializedObject.FindProperty(nameof(EventTrigger.OnTriggerStayGOEvent));
            OnTriggerExitEvent = serializedObject.FindProperty(nameof(EventTrigger.OnTriggerExitEvent));
            OnTriggerExitGOEvent = serializedObject.FindProperty(nameof(EventTrigger.OnTriggerExitGOEvent));
            PlayOnlyFirst = serializedObject.FindProperty(nameof(EventTrigger.PlayOnlyFirst));

            _tags = UnityEditorInternal.InternalEditorUtility.tags;
            for (int i = 0; i < TargetTags.arraySize; i++)
            {
                var tag = TargetTags.GetArrayElementAtIndex(i).stringValue;

                int tagIndex = Array.IndexOf(_tags, tag);

                if (tagIndex >= 0)
                {
                    _tagMask |= 1 << tagIndex;
                }
            }
            
            _et = target as EventTrigger;
        }

        protected override void OverrideOnReady()
        {
            EditorGUI.BeginChangeCheck();
            _tagMask = EditorGUILayout.MaskField("TargetTags", _tagMask, _tags);
            if (EditorGUI.EndChangeCheck())
            {
                TargetTags.ClearArray();

                int tagMask = _tagMask;
                int tagIndex = 0;
                int arrayIndex = 0;

                while (tagMask > 0)
                {
                    if (tagMask % 2 == 1)
                    {
                        TargetTags.InsertArrayElementAtIndex(arrayIndex);
                        TargetTags.GetArrayElementAtIndex(arrayIndex).stringValue = _tags[tagIndex];
                        arrayIndex++;
                    }

                    tagMask /= 2;
                    tagIndex++;
                }
            }
                
            EditorGUILayout.PropertyField(TargetLayerMask);
            EditorGUILayout.PropertyField(OnTriggerEnterEvent);
            EditorGUILayout.PropertyField(OnTriggerEnterGOEvent);
            EditorGUILayout.PropertyField(OnTriggerStayEvent);
            EditorGUILayout.PropertyField(OnTriggerStayGOEvent);
            EditorGUILayout.PropertyField(OnTriggerExitEvent);
            EditorGUILayout.PropertyField(OnTriggerExitGOEvent);
            EditorGUILayout.PropertyField(PlayOnlyFirst);
        }

        protected override void ChangeColliderType()
        {
            base.ChangeColliderType();
            DestroyImmediate(Rigidbody.objectReferenceValue);
        }

        protected override void Set3DColliderSettings()
        {
            base.Set3DColliderSettings();
            Rigidbody.objectReferenceValue = _et.gameObject.AddComponent<Rigidbody>();
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(Rigidbody.objectReferenceValue, false);
        }
        
        protected override void Set2DColliderSettings()
        {
            base.Set2DColliderSettings();
            Rigidbody.objectReferenceValue = _et.gameObject.AddComponent<Rigidbody2D>();
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(Rigidbody.objectReferenceValue, false);
        }
    }
}