using System;
using System.Collections.Generic;
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

            if (currentState != string.Empty)
            {
                Type parentType = property.serializedObject.targetObject.GetType();
                var fi = parentType.GetField(property.propertyPath);
                _sm = (StateMachine)fi.GetValue(property.serializedObject.targetObject);
                _method = fi.FieldType.GetMethod("ChangeTo", new Type[]{typeof(string), typeof(bool)});
                
                _dropdownField = new DropdownField(property.name,new List<string>(_sm.StateNames),currentState);
                
                _sm.OnStateChanged += OnStateChanged;
                _dropdownField.RegisterValueChangedCallback(OnInspectorValueChanged);

                if (_sm.GetType().IsAssignableFrom(typeof(StateMachine<>)))
                {
                    _dropdownField.RegisterCallback<ContextClickEvent>(OnContextClick);
                }

                return _dropdownField;
            }
            else
            {
                return new Label($"{property.name} is not constructed yet");
            }
        }

        private void OnStateChanged(string newState)
        {
            _dropdownField.value = newState;
        }
        
        private void OnInspectorValueChanged(ChangeEvent<string> evt)
        {
            _method.Invoke(_sm, new object[] {evt.newValue, false});
        }
        
        private void OnContextClick(ContextClickEvent evt)
        {
            GenericMenu contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Reset"), false, () => _sm.Reset());
            contextMenu.ShowAsContext();
        }
    }
}