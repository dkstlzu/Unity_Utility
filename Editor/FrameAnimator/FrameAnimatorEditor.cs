using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace dkstlzu.Utility
{
    [CustomPropertyDrawer(typeof(FrameAnimator))]
    public class FrameAnimatorEditor : PropertyDrawer
    {
        private VisualElement root;
        private SerializedProperty _property;
        private SerializedProperty _targetAnimationProperty;
        private Label _nameLabel;
        private Label _frameLabel;
        private Label _spriteLabel;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            EditorApplication.update += UpdateCurrentAnimationInfo;
            _property = property;
            
            var root = new VisualElement();
            
            root.Add(new PropertyField(property.FindPropertyRelative(nameof(FrameAnimator.DefaultAnimationIndex))));
            root.Add(new PropertyField(property.FindPropertyRelative(nameof(FrameAnimator.PlayOnAwake))));
            root.Add(new PropertyField(property.FindPropertyRelative(nameof(FrameAnimator.LoopCurrent))));
            root.Add(new PropertyField(property.FindPropertyRelative(nameof(FrameAnimator.AutoTransition))));
            root.Add(new PropertyField(property.FindPropertyRelative("_renderer")));
            root.Add(new PropertyField(property.FindPropertyRelative("_animations")));
            
            Foldout currentAnimation = new Foldout();
            currentAnimation.text = "현재 애니메이션";

            int targetAnimationIndex = property.FindPropertyRelative("_currentAnimationIndex").intValue;

            string labelName = string.Empty;
            int frameNum = 0;
            int spriteIndex = 0;
            
            if (property.FindPropertyRelative("_animations").arraySize > 0)
            {
                _targetAnimationProperty = property.FindPropertyRelative("_animations").GetArrayElementAtIndex(targetAnimationIndex);

                root.Unbind();
                root.TrackPropertyValue(property.FindPropertyRelative("_currentAnimationIndex"), UpdateTargetAnimation);

                labelName = _targetAnimationProperty.FindPropertyRelative("Name").stringValue;
                frameNum = _targetAnimationProperty.FindPropertyRelative("_currentFrame").intValue;
                spriteIndex = _targetAnimationProperty.FindPropertyRelative("_currentSpriteIndex").intValue;
            }
            
            _nameLabel = new Label(labelName);
            currentAnimation.Add(_nameLabel);
            
            _frameLabel = new Label($"Frame : {frameNum}");
            currentAnimation.Add(_frameLabel);

            _spriteLabel = new Label($"Sprite index : {spriteIndex}");
            currentAnimation.Add(_spriteLabel);
            
            root.Add(currentAnimation);

            return root;
        }

        ~FrameAnimatorEditor()
        {
            EditorApplication.update -= UpdateCurrentAnimationInfo;
        }

        void UpdateTargetAnimation(SerializedProperty property)
        {
            _targetAnimationProperty = _property.FindPropertyRelative("_animations")
                .GetArrayElementAtIndex(property.intValue);
        }

        void UpdateCurrentAnimationInfo()
        {
            try
            {
                _nameLabel.text = $"이름 : {_targetAnimationProperty.FindPropertyRelative("Name").stringValue}";
                _frameLabel.text = $"프레임 : {_targetAnimationProperty.FindPropertyRelative("_currentFrame").intValue}";
                _spriteLabel.text = $"이미지 번호 : {_targetAnimationProperty.FindPropertyRelative("_currentSpriteIndex").intValue}";
            }
            catch (Exception)
            {
                EditorApplication.update -= UpdateCurrentAnimationInfo;
            }
        }
    }

    [CustomEditor(typeof(FrameAnimatorController))]
    public class FrameAnimatorControllerEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            return root;
        }
    }
}