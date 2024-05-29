using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace dkstlzu.Utility
{
    [CustomPropertyDrawer(typeof(SceneCallbackEventHandler))]
    public class SceneCallbackHandlerPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty _property;
        private SerializedProperty _sceneNameProperty;
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            _property = property;
            
            VisualElement root = new VisualElement();

            PropertyField sceneAssetField = new PropertyField(property.FindPropertyRelative(nameof(SceneCallbackEventHandler.Scene)));
            sceneAssetField.RegisterValueChangeCallback(OnSceneAssetChange);
            root.Add(sceneAssetField);
            root.Add(new PropertyField(property.FindPropertyRelative("OnLoad")));
            root.Add(new PropertyField(property.FindPropertyRelative("OnUnload")));

            _sceneNameProperty = property.FindPropertyRelative("_sceneName");
            
            return root;
        }

        private void OnSceneAssetChange(SerializedPropertyChangeEvent evt)
        {
            _property.serializedObject.Update();
            
            SceneAsset newAsset = evt.changedProperty.objectReferenceValue as SceneAsset;
            _sceneNameProperty.stringValue = newAsset?.name ?? "";
            
            _property.serializedObject.ApplyModifiedProperties();
        }
    }
}