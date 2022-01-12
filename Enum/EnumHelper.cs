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
    }
}
