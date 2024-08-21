using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;

namespace dkstlzu.Utility
{
    [CustomPropertyDrawer(typeof(StateMachine<>))]
    public class StateMachinePropertyDrawer : PropertyDrawer
    {
        private object _obj;
        private MethodInfo _method;
        private Type _enumType;
        private SerializedProperty _stateProperty;
        private DropdownField _field;
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            _stateProperty = property.FindPropertyRelative("_currentState");

            Type parentType = property.serializedObject.targetObject.GetType();
            var fi = parentType.GetField(property.propertyPath);
            _obj = fi.GetValue(property.serializedObject.targetObject);
            _method = fi.FieldType.GetMethod("ChangeTo");
            _enumType = fi.FieldType.GenericTypeArguments[0];
            
            _field = new DropdownField(_enumType.Name + " StateMachine", new List<string>(_stateProperty.enumNames), _stateProperty.enumValueIndex);

            _field.RegisterValueChangedCallback(OnValueChanged);
            
            return _field;
        }

        private void OnValueChanged(ChangeEvent<string> evt)
        {
            _stateProperty.enumValueIndex = _field.index;
            _method.Invoke(_obj, new object[] {Enum.ToObject(_enumType, _field.index), false });
        }
    }
}