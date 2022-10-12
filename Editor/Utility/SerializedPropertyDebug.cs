using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace dkstlzu.Utility
{
    public static class SerializedPropertyDebug
    {
        public static void LogProperties(SerializedObject so, bool includeChildren = true) {
            // Shows all the properties in the serialized object with name and type
            // You can use this to learn the structure
            so.Update();
            SerializedProperty propertyLogger = so.GetIterator();
            while(true) {
                Debug.Log("name = " + propertyLogger.name + " type = " + propertyLogger.type);
                if(!propertyLogger.Next(includeChildren)) break;
            }
        }
        // variablePath may have a structure like this:
        // "meshData.Array.data[0].vertexColors"
        // So it uses FindProperty to get data from a specific field in an object array
        public static void SetSerializedProperty(UnityEngine.Object obj, string variablePath, object variableValue) {
                SerializedObject so = new SerializedObject(obj);
                SerializedProperty sp = so.FindProperty(variablePath);
                if(sp == null) {
                    Debug.Log("Error setting serialized property! Variable path: \"" + variablePath + "\" not found in object!");
                    return;
                }
                
                so.Update(); // refresh the data
                
                //SerializedPropertyType type = sp.propertyType; // get the property type
                System.Type valueType = variableValue.GetType(); // get the type of the incoming value
                
                if(sp.isArray && valueType != typeof(string)) { // serialized property is an array, except string which is also an array
                    // assume the incoming value is also an array
                    if(!WriteSerializedArray(sp, variableValue)) return; // write the array
                } else { // not an array
                    if(!WriteSerialzedProperty(sp, variableValue)) return; // write the value to the property
                }
                
                so.ApplyModifiedProperties(); // apply the changes
        }
            
        private static bool WriteSerialzedProperty(SerializedProperty sp, object variableValue) {
            // Type the property and fill with new value
            SerializedPropertyType type = sp.propertyType; // get the property type
            
            if(type == SerializedPropertyType.Integer) {
                int it = (int)variableValue;
                if(sp.intValue != it) {
                    sp.intValue = it;
                }
            } else if(type == SerializedPropertyType.Boolean) {
                bool b = (bool)variableValue;
                if(sp.boolValue != b) {
                    sp.boolValue = b;
                }
            } else if(type == SerializedPropertyType.Float) {
                float f = (float)variableValue;
                if(sp.floatValue != f) {
                    sp.floatValue = f;
                }
            } else if(type == SerializedPropertyType.String) {
                string s = (string)variableValue;
                if(sp.stringValue != s) {
                    sp.stringValue = s;
                }
            } else if(type == SerializedPropertyType.Color) {
                Color c = (Color)variableValue;
                if(sp.colorValue != c) {
                    sp.colorValue = c;
                }
            } else if(type == SerializedPropertyType.ObjectReference) {
                Object o = (Object)variableValue;
                if(sp.objectReferenceValue != o) {
                    sp.objectReferenceValue = o;
                }
            } else if(type == SerializedPropertyType.LayerMask) {
                int lm = (int)variableValue;
                if(sp.intValue != lm) {
                    sp.intValue = lm;
                }
            } else if(type == SerializedPropertyType.Enum) {
                int en = (int)variableValue;
                if(sp.enumValueIndex != en) {
                    sp.enumValueIndex = en;
                }
            } else if(type == SerializedPropertyType.Vector2) {
                Vector2 v2 = (Vector2)variableValue;
                if(sp.vector2Value != v2) {
                    sp.vector2Value = v2;
                }
            } else if(type == SerializedPropertyType.Vector3) {
                Vector3 v3 = (Vector3)variableValue;
                if(sp.vector3Value != v3) {
                    sp.vector3Value = v3;
                }
            } else if(type == SerializedPropertyType.Rect) {
                Rect r = (Rect)variableValue;
                if(sp.rectValue != r) {
                    sp.rectValue = r;
                }
            } else if(type == SerializedPropertyType.ArraySize) {
                int aSize = (int)variableValue;
                if(sp.intValue != aSize) {
                    sp.intValue = aSize;
                }
            } else if(type == SerializedPropertyType.Character) {
                int ch = (int)variableValue;
                if(sp.intValue != ch) {
                    sp.intValue = ch;
                }
            } else if(type == SerializedPropertyType.AnimationCurve) {
                AnimationCurve ac = (AnimationCurve)variableValue;
                if(sp.animationCurveValue != ac) {
                    sp.animationCurveValue = ac;
                }
            } else if(type == SerializedPropertyType.Bounds) {
                Bounds bounds = (Bounds)variableValue;
                if(sp.boundsValue != bounds) {
                    sp.boundsValue = bounds;
                }
            } else {
                Debug.Log("Unsupported SerializedPropertyType \"" + type.ToString() + " encoutered!");
                return false;
            }
            return true;
        }
        
        private static bool WriteSerializedArray(SerializedProperty sp, object arrayObject) {
            System.Array[] array = (System.Array[])arrayObject; // cast to array
            
            sp.Next(true); // skip generic field
            sp.Next(true); // advance to array size field
            
            // Set the array size
            if(!WriteSerialzedProperty(sp, array.Length)) return false;
            
            sp.Next(true); // advance to first array index
            
            // Write values to array
            int lastIndex = array.Length - 1;
            for(int i = 0; i < array.Length; i++) {
                if(!WriteSerialzedProperty(sp, array[i])) return false; // write the value to the property
                if(i < lastIndex) sp.Next(false); // advance without drilling into children            }
            
            }
            return true;
        }

        public static void LogAllValues(SerializedObject serializedObject)
        {
            LogAllValues(serializedObject.GetIterator());
        }
        
        // A way to see everything a SerializedProperty object contains in case you don't
        // know what type is stored.
        public static void LogAllValues(SerializedProperty serializedProperty) {
            SerializedProperty sp = serializedProperty.Copy();
            StringBuilder log = new StringBuilder();
            log.AppendLine("Log of All values in serialized property.");
            
            log.AppendLine("PROPERTY: name = " + sp.name + " type = " + sp.type);
            log.AppendLine("animationCurveValue = " + sp.animationCurveValue);
            log.AppendLine("arraySize = " + sp.arraySize);
            log.AppendLine("boolValue = " + sp.boolValue);
            log.AppendLine("boundsValue = " + sp.boundsValue);
            log.AppendLine("colorValue = " + sp.colorValue);
            log.AppendLine("depth = " + sp.depth);
            log.AppendLine("editable = " + sp.editable);
            log.AppendLine("enumNames = " + sp.enumNames);
            log.AppendLine("enumValueIndex = " + sp.enumValueIndex);
            log.AppendLine("floatValue = " + sp.floatValue);
            log.AppendLine("hasChildren = " + sp.hasChildren);
            log.AppendLine("hasMultipleDifferentValues = " + sp.hasMultipleDifferentValues);
            log.AppendLine("hasVisibleChildren = " + sp.hasVisibleChildren);
            log.AppendLine("intValue = " + sp.intValue);
            log.AppendLine("isAnimated = " + sp.isAnimated);
            log.AppendLine("isArray = " + sp.isArray);
            log.AppendLine("isExpanded = " + sp.isExpanded);
            log.AppendLine("isInstantiatedPrefab = " + sp.isInstantiatedPrefab);
            log.AppendLine("name = " + sp.name);
            log.AppendLine("objectReferenceInstanceIDValue = " + sp.objectReferenceInstanceIDValue);
            log.AppendLine("objectReferenceValue = " + sp.objectReferenceValue);
            log.AppendLine("prefabOverride = " + sp.prefabOverride);
            log.AppendLine("propertyPath = " + sp.propertyPath);
            log.AppendLine("propertyType = " + sp.propertyType);
            log.AppendLine("quaternionValue = " + sp.quaternionValue);
            log.AppendLine("rectValue = " + sp.rectValue);
            log.AppendLine("serializedObject = " + sp.serializedObject);
            log.AppendLine("stringValue = " + sp.stringValue);
            log.AppendLine("tooltip = " + sp.tooltip);
            log.AppendLine("type = " + sp.type);
            log.AppendLine("vector2Value = " + sp.vector2Value);
            log.AppendLine("vector3Value = " + sp.vector3Value);
            
            Debug.Log(log.ToString());
        }

        public static void LogAllPropertyPath(SerializedObject serializedObject)
        {
            LogAllPropertyPath(serializedObject.GetIterator());
        }

        public static void LogAllPropertyPath(SerializedProperty serializedProperty)
        {
            SerializedProperty sp = serializedProperty.Copy();
            StringBuilder log = new StringBuilder("Log of All path in serialized property.\n");

            do
            {
                log.AppendLine(sp.propertyPath);
            }
            while(sp.Next(true));

            Debug.Log(log.ToString());
        }
    }
}