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
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var foldout = new Foldout();
            foldout.text = "Handler";
            
            PropertyField sceneAssetField = new PropertyField(property.FindPropertyRelative(nameof(SceneCallbackEventHandler.Scene)));
            SceneCallbackEventHandler handler = (SceneCallbackEventHandler)EditorHelper.GetTargetObjectOfProperty(property);

            sceneAssetField.RegisterValueChangeCallback(OnSceneAssetChange);
            
            foldout.RegisterCallback<ContextClickEvent>(OnContextClick);
            foldout.Add(sceneAssetField);
            foldout.Add(new PropertyField(property.FindPropertyRelative(nameof(SceneCallbackEventHandler.OnLoad))));
            foldout.Add(new PropertyField(property.FindPropertyRelative(nameof(SceneCallbackEventHandler.OnUnload))));
            return foldout;
        
            void OnContextClick(ContextClickEvent evt)
            {
                GenericMenu contextMenu = new GenericMenu();

                EventArg arg = new EventArg();
                arg.SceneName = handler.SceneName;

                arg.Action = handler.OnLoad.Invoke;
                contextMenu.AddItem(new GUIContent("RegisterLoad"), false, 
                    (obj) => SceneCallback.RegisterLoadEvent(((EventArg)obj).SceneName, ((EventArg)obj).Action)
                    , arg);
                arg.Action = handler.OnUnload.Invoke;
                contextMenu.AddItem(new GUIContent("RegisterUnload"), false, 
                    (obj) => SceneCallback.RegisterUnloadEvent(((EventArg)obj).SceneName, ((EventArg)obj).Action)
                    , arg);
                arg.Action = handler.OnLoad.Invoke;
                contextMenu.AddItem(new GUIContent("UnregisterLoad"), false, 
                    (obj) => SceneCallback.UnregisterLoadEvent(((EventArg)obj).SceneName, ((EventArg)obj).Action)
                    , arg);
                arg.Action = handler.OnUnload.Invoke;
                contextMenu.AddItem(new GUIContent("UnregisterUnload"), false, 
                    (obj) => SceneCallback.UnregisterUnloadEvent(((EventArg)obj).SceneName, ((EventArg)obj).Action)
                    , arg);

                contextMenu.ShowAsContext();
            }

            void OnSceneAssetChange(SerializedPropertyChangeEvent evt)
            {
                SceneAsset newAsset = evt.changedProperty.objectReferenceValue as SceneAsset;
                handler.SceneName = newAsset?.name ?? "";
                foldout.text = newAsset?.name ?? "Handler";
            }
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
    }
}