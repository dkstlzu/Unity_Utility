using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace dkstlzu.Utility
{
    [CustomPropertyDrawer(typeof(SceneCallbackEventHandler))]
    public class SceneCallbackHandlerPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty _property;
        private SerializedProperty _sceneNameProperty;
        private Foldout _foldout;
        private UnityEvent _loadEvent;
        private UnityEvent _unloadEvent;
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            _property = property;
            
            VisualElement root = new VisualElement();

            _foldout = new Foldout();
            _foldout.text = "Handler";
            root.Add(_foldout);
            PropertyField sceneAssetField = new PropertyField(property.FindPropertyRelative(nameof(SceneCallbackEventHandler.Scene)));
            sceneAssetField.RegisterValueChangeCallback(OnSceneAssetChange);
            _foldout.Add(sceneAssetField);
            _foldout.Add(new PropertyField(property.FindPropertyRelative(nameof(SceneCallbackEventHandler.OnLoad))));
            _foldout.Add(new PropertyField(property.FindPropertyRelative(nameof(SceneCallbackEventHandler.OnUnload))));

            var targetObject = property.serializedObject.targetObject;
            var targetObjectClassType = targetObject.GetType();
            var field = targetObjectClassType.GetField(property.propertyPath);
            if (field != null)
            {
                object value = field.GetValue(targetObject);
                if (value is SceneCallbackEventHandler handler)
                {
                    _loadEvent = handler.OnLoad;
                    _unloadEvent = handler.OnUnload;
            
                    root.RegisterCallback<ContextClickEvent>(OnContextClick);
                }
            }
            
            _sceneNameProperty = property.FindPropertyRelative("_sceneName");
            
            return root;
        }

        struct EventArg
        {
            public string SceneName;
            public Action Action;

            public EventArg(string sceneName, Action action)
            {
                SceneName = sceneName;
                Action = action;
            }
        }
        
        private void OnContextClick(ContextClickEvent evt)
        {
            GenericMenu contextMenu = new GenericMenu();

            EventArg arg = new EventArg();
            arg.SceneName = _sceneNameProperty.stringValue;

            arg.Action = _loadEvent.Invoke;
            contextMenu.AddItem(new GUIContent("RegisterLoad"), false, 
                (obj) => SceneCallback.RegisterLoadEvent(((EventArg)obj).SceneName, ((EventArg)obj).Action)
                , arg);
            arg.Action = _unloadEvent.Invoke;
            contextMenu.AddItem(new GUIContent("RegisterUnload"), false, 
                (obj) => SceneCallback.RegisterUnloadEvent(((EventArg)obj).SceneName, ((EventArg)obj).Action)
                , arg);
            arg.Action = _loadEvent.Invoke;
            contextMenu.AddItem(new GUIContent("UnregisterLoad"), false, 
                (obj) => SceneCallback.UnregisterLoadEvent(((EventArg)obj).SceneName, ((EventArg)obj).Action)
                , arg);
            arg.Action = _unloadEvent.Invoke;
            contextMenu.AddItem(new GUIContent("UnregisterUnload"), false, 
                (obj) => SceneCallback.UnregisterUnloadEvent(((EventArg)obj).SceneName, ((EventArg)obj).Action)
                , arg);

            contextMenu.ShowAsContext();
        }

        private void OnSceneAssetChange(SerializedPropertyChangeEvent evt)
        {
            _property.serializedObject.Update();
            
            SceneAsset newAsset = evt.changedProperty.objectReferenceValue as SceneAsset;
            _sceneNameProperty.stringValue = newAsset?.name ?? "";
            _foldout.text = newAsset?.name ?? "Handler";
            
            _property.serializedObject.ApplyModifiedProperties();
        }
    }
}