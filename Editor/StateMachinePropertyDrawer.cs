using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace dkstlzu.Utility
{
    [CustomPropertyDrawer(typeof(StateMachine))]
    public class StateMachinePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var propertyField = new PropertyField(property.FindPropertyRelative("_currentState"), "StateMachine");
            propertyField.SetEnabled(false);

            return propertyField;
        }
    }
    
    [CustomPropertyDrawer(typeof(StateMachine<>))]
    public class EnumStateMachinePropertyDrawer : PropertyDrawer
    {
        private StateMachine _sm;
        private MethodInfo _method;
        private DropdownField _dropdownField;
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Type parentType = property.serializedObject.targetObject.GetType();
            var fi = parentType.GetField(property.propertyPath);
            _sm = (StateMachine)fi.GetValue(property.serializedObject.targetObject);
            var enumType = fi.FieldType.GenericTypeArguments[0];
            _method = fi.FieldType.GetMethod("ChangeTo", new Type[]{typeof(string), typeof(bool)});
            
            _dropdownField = new DropdownField(enumType.Name + " StateMachine", 
                new List<string>(Enum.GetNames(enumType)), property.FindPropertyRelative("_currentState").stringValue);

            _sm.OnStateChanged += OnStateChanged;
            _dropdownField.RegisterValueChangedCallback(OnInspectorValueChanged);
            
            return _dropdownField;
        }

        private void OnStateChanged(string newState)
        {
            _dropdownField.value = newState;
        }
        
        private void OnInspectorValueChanged(ChangeEvent<string> evt)
        {
            _method.Invoke(_sm, new object[] {evt.newValue, false});
        }
    }
}