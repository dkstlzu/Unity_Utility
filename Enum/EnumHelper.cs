using System.Collections;
using System.Collections.Generic;
using System;

namespace Utility
{
    public static class EnumHelper
    {
        public static void ClapIndexOfEnum<EnumType>(int from, int to, out int startIndex, out int endIndex) where EnumType : Enum
        {
            UnityEngine.MonoBehaviour.print("EnumHelper Claping Index of " + typeof(EnumType));
            int enumStart = from;
            int enumEnd = to;
            startIndex = -1;
            endIndex = -1;

            int[] intarr = (int[])Enum.GetValues(typeof(EnumType));
            int index = -1;
            bool startCheck=false, endCheck=false;
            foreach (int i in intarr)
            {
                index++;

                if (startCheck && endCheck) break;

                if (i >= enumStart && !startCheck)
                {
                    startIndex = index;
                    startCheck = true;
                }

                if (i >= enumEnd && !endCheck)
                {
                    endIndex = index-1;
                    endCheck = true;
                }
            }

            if (!endCheck)
            {
                endIndex = index;
            }
        }

        public static Type GetEnumType(string enumName)
        {
            Type type = Type.GetType(enumName + ", Assembly-CSharp-firstpass", true, true);
            if (type == null) Type.GetType(enumName + ", Assembly-CSharp", true, true);

            if (type == null) UnityEngine.Debug.LogWarning("Wrong EnumName in EnumHelper.GetEnumType(). Check Assembly");
            return type;
        }

        public static int GetFirstValue(Type type)
        {
            int[] values = (int[])Enum.GetValues(type);
            return values[0];
        }

        public static int GetIndexOf(Enum enumValue)
        {
            return Array.IndexOf(Enum.GetValues(enumValue.GetType()), enumValue);
        }
        public static int GetIndexOf(string enumValueString, Type type)
        {
            return Array.IndexOf(Enum.GetValues(type), Enum.Parse(type, enumValueString) as Enum);
        }
        public static int GetIndexOf<TEnum>(string enumValueString)
        {
            return Array.IndexOf(Enum.GetValues(typeof(TEnum)), Enum.Parse(typeof(TEnum), enumValueString) as Enum);
        }
    }
}
