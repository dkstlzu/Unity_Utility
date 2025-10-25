using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(EditorOnlyInspector))]
public class EditorOnlyInspectorEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();

        SerializedProperty property = serializedObject.FindProperty("obj");
        PropertyField propertyField = new PropertyField(property, property.managedReferenceValue.GetType().Name);
        root.Add(propertyField);
        
        return root;
    }
}
