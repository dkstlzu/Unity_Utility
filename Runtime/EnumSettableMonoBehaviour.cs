using System;
using UnityEngine;

namespace dkstlzu.Utility
{
    /// <summary>
    /// 원하는 EnumType에 따라 dynamic하게 dropdown 메뉴를 만들 수 있는 MonoBehaviour
    /// </summary>
    public class EnumSettableMonoBehaviour : MonoBehaviour
    {
        [Tooltip("Make Name form as [NameSpace].[EnumName]")]
        [HideInInspector]
        public string EnumName;
        [HideInInspector]
        public string _EnumValue;
        [HideInInspector]
        public bool EnumNameCorrect;
        
        public Enum EnumValue
        {
            get
            {
                Enum val;
                try
                {
                    Type enumType = EnumHelper.GetEnumType(EnumName);
                    if (!Enum.IsDefined(enumType, _EnumValue))
                        _EnumValue = ((Enum)Enum.GetValues(enumType).GetValue(0)).ToString();
                    val = Enum.Parse(enumType, _EnumValue) as Enum;
                } catch
                {
                    Debug.LogWarning($"EnumSettableMonoBehaviour ({gameObject.name}) EnumName or EnumValue is wrong. Check again.");
                    return null;
                }
                return val;
            }
        }

    }
}