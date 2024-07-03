using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace dkstlzu.Utility
{
    [CustomEditor(typeof(RandomAudioClip))]
    public class RandomAudioClipEditor : Editor
    {
        private VisualElement _root;
        private RandomAudioClip _target;
        
        public override VisualElement CreateInspectorGUI()
        {
            _target = target as RandomAudioClip;
            _root = new VisualElement();
            
            InspectorElement.FillDefaultInspector(_root, serializedObject, this);

            var e = _root.Q<PropertyField>("PropertyField:Clips");
            e.RegisterCallback<DragUpdatedEvent>(EnableDrag);
            e.RegisterCallback<DragPerformEvent>(OnDrag);
            
            return _root;
        }

        private void EnableDrag(DragUpdatedEvent evt)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        }

        private void OnDrag(DragPerformEvent evt)
        {
            serializedObject.Update();
            
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is AudioClip clip)
                {
                    _target.Clips.Add(clip);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}