using System.Collections;
using System.Collections.Generic;
using System;

namespace Utility
{
    public static class EnumHelper
    {
        public static void ClapIndexOfEnum<EnumType>(int from, int to, out int startIndex, out int endIndex)
        {
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

                if (i > enumEnd && !endCheck)
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
    }
}
