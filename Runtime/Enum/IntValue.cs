using System;
using System.Reflection;

namespace dkstlzu.Utility
{
    [AttributeUsage(AttributeTargets.Field)]
    public class IntValue : System.Attribute
    {
        private int _value; 

        public IntValue(int value) 
        { 
            _value = value; 
        } 

        public int Value 
        { 
            get {return _value;}
        } 

        public static int GetEnumIntValue(Object value)
        {
            int output = 0;
            
            Type type = value.GetType();
            
            FieldInfo fi = type.GetField(value.ToString());
            IntValue[] attrs = fi.GetCustomAttributes(typeof(IntValue), false) as IntValue[];
            
            if(attrs.Length > 0)
            {
                output = attrs[0].Value;
            }
                
            return output;
        }

        public static T GetEnumValue<T>(int value) where T : Enum
        {
            Type type = typeof(T);

            T[] TArr = Enum.GetValues(typeof(T)) as T[];

            foreach (T t in TArr)
            {
                FieldInfo Tfi = type.GetField(t.ToString());
                
                IntValue attr = Tfi.GetCustomAttribute<IntValue>();

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
