using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace dkstlzu.Utility
{
    [CustomPropertyDrawer(typeof(SoundManager.BGMInfo))]
    public class BGMInfoPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var foldout = new Foldout();
            foldout.text = "BGMInfo";
            var sceneAssetField = new PropertyField(property.FindPropertyRelative(nameof(SoundManager.BGMInfo.Scene)));
            var bgmClipProperty = property.FindPropertyRelative(nameof(SoundManager.BGMInfo.BGMClip));
            var info = (SoundManager.BGMInfo)EditorHelper.GetTargetObjectOfProperty(property);
            
            sceneAssetField.RegisterValueChangeCallback(OnSceneAssetChange);

            foldout.Add(sceneAssetField);
            // foldout.Add(new PropertyField(sceneNameProperty));
            foldout.Add(new PropertyField(bgmClipProperty));

            return foldout;
            
            void OnSceneAssetChange(SerializedPropertyChangeEvent evt)
            {
                SceneAsset newAsset = evt.changedProperty.objectReferenceValue as SceneAsset;
                info.SceneName = newAsset != null ? newAsset.name : "";
                foldout.text = newAsset != null ? newAsset.name + " BGM" : "BGMInfo";
            }
        }
    }
}