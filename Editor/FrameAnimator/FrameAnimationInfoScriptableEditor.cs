using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace dkstlzu.Utility
{
    [CustomEditor(typeof(FrameAnimationInfoScriptable))]
    public class FrameAnimationInfoScriptableEditor : UnityEditor.Editor
    {
        private VisualElement root;
        private FrameAnimationInfoScriptable info;
        
        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();

            info = serializedObject.targetObject as FrameAnimationInfoScriptable;

            Validate();
            
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            root.Query<PropertyField>().ForEach((field) =>
            {
                field.RegisterValueChangeCallback((e) =>
                {
                    Validate();
                });
            });

            return root;
        }

        private void Validate()
        {
            if (isValid())
            {
                removeHelpBox();
            }
            else
            {
                insertHelpBox();
            }
        }

        bool isValid()
        {
            return info.IsValid();
        }

        void insertHelpBox()
        {
            if (root.Q<HelpBox>() == null)
            {
                HelpBox box = new HelpBox("Sprite 개수랑 Frame Data 개수가 다르거나 할당되지 않았습니다.", HelpBoxMessageType.Error);
                root.Insert(0, box);
            }
        }

        void removeHelpBox()
        {
            if (root.Q<HelpBox>() != null)
            {
                root.Remove(root.Q<HelpBox>());
            }
        }
    }
}