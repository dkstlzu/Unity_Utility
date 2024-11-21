using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace dkstlzu.Utility
{
    public class TargetPropertyAttribute : Attribute
    {
        public string Value;

        public TargetPropertyAttribute(string value)
        {
            Value = value;
        }
    }
    
    public class TargetLabelAttribute : Attribute
    {
        public string Value;
        
        public TargetLabelAttribute(string value)
        {
            Value = value;
        }
    }
    
    public class ListWrapperPropertyDrawer<T> : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();

            string propertyName = "List";
            string labelName = "List";
            
            foreach (var at in GetType().GetCustomAttributes(typeof(TargetPropertyAttribute), true))
            {
                TargetPropertyAttribute tpa = at as TargetPropertyAttribute;
                propertyName = tpa.Value;
            }
            
            foreach (var at in GetType().GetCustomAttributes(typeof(TargetLabelAttribute), true))
            {
                TargetLabelAttribute tla = at as TargetLabelAttribute;
                labelName = tla.Value;
            }
            
            var listProperty = property.FindPropertyRelative(propertyName);
            var propertyField = new PropertyField(listProperty, labelName);
            root.Add(propertyField);
            
            return root;
        }
    }
    
    [CustomPropertyDrawer(typeof(DoubleList<int>))]
    public class IntDoubleListWrapperPropertyDrawer : ListWrapperPropertyDrawer<int> {}
    [CustomPropertyDrawer(typeof(DoubleList<int>.ListElement))]
    [TargetProperty("Element")]
    [TargetLabel("Element")]
    public class IntDoubleListElementPropertyDrawer : ListWrapperPropertyDrawer<int> {}
    
    [CustomPropertyDrawer(typeof(DoubleList<Rect>))]
    public class RectDoubleListWrapperPropertyDrawer : ListWrapperPropertyDrawer<Rect> {}
    [CustomPropertyDrawer(typeof(DoubleList<Rect>.ListElement))]
    [TargetProperty("Element")]
    [TargetLabel("Element")]
    public class RectDoubleListElementPropertyDrawer : ListWrapperPropertyDrawer<Rect> {}
}