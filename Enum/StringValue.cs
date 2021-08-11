using System;
using System.Reflection;

namespace Utility
{
    public class StringValue : System.Attribute 
    { 

        private string _value; 

        public StringValue(string value) 
        { 
            _value = value; 
        } 

        public string Value 
        { 
            get {return _value;}
        } 

        public static string GetStringValue(Object value)
        {
            string output = null;
            
            Type type = value.GetType();
            
            FieldInfo fi = type.GetField(value.ToString());
            StringValue[] attrs = fi.GetCustomAttributes(typeof(StringValue), false) as StringValue[];
            
            if(attrs.Length > 0)
            {
                output = attrs[0].Value;
            }
                
            return output;
        }

        public static T GetEnumValue<T>(string value) where T : Enum
        {
            Type type = typeof(T);

            T[] TArr = Enum.GetValues(typeof(T)) as T[];

            foreach (T t in TArr)
            {
                FieldInfo Tfi = type.GetField(t.ToString());
                
                StringValue attr = Tfi.GetCustomAttribute<StringValue>();

                if (attr == null) continue;

                if (attr.Value == value)
                {
                    return t;
                }
            }
            
            return TArr[0];
        }
    }
}