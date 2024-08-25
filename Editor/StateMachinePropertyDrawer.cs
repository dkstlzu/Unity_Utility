using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace dkstlzu.Utility
{
    [CustomPropertyDrawer(typeof(StateMachine), true)]
    public class StateMachinePropertyDrawer : PropertyDrawer
    {
        private StateMachine _sm;
        private MethodInfo _method;
        private DropdownField _dropdownField;
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var currentState = property.FindPropertyRelative("_currentState").stringValue;

            _sm = (StateMachine)EditorHelper.GetTargetObjectOfProperty(property);

            if (_sm.Keys.Contains(currentState))
            {
                _method = typeof(StateMachine).GetMethod("ChangeTo", new Type[]{typeof(string), typeof(bool)});
                
                _dropdownField = new DropdownField(property.displayName,new List<string>(_sm.Keys),currentState);
                
                _sm.OnStateChanged += OnStateChanged;
                _dropdownField.RegisterValueChangedCallback(OnInspectorValueChanged);

                _dropdownField.RegisterCallback<ContextClickEvent>(OnContextClick);

                return _dropdownField;
            }
            else
            {
                return new Label($"{property.displayName} is not constructed yet");
            }
        }

        private void OnStateChanged(string previousState, string newState)
        {
            _dropdownField.value = newState;
        }
        
        private void OnInspectorValueChanged(ChangeEvent<string> evt)
        {
            _sm.ChangeTo(evt.newValue, true);
        }
        
        private void OnContextClick(ContextClickEvent evt)
        {
            GenericMenu contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Reset"), false, () => _sm.Reset());
            contextMenu.AddItem(new GUIContent("EnableLog"), _sm.EnableLog, () => _sm.EnableLog = !_sm.EnableLog);
            contextMenu.ShowAsContext();
        }
    }
}