﻿using System;
using UnityEngine;

namespace Utility
{
    public class EnumSettableMonoBehaviour : MonoBehaviour
    {
        [SerializeField] protected string enumName;
        [SerializeField] protected string enumValue;
        [SerializeField] protected bool enumNameCorrect;
        public Enum EnumValue
        {
            get
            {
                Enum val;
                try
                {
                    Type enumType = EnumHelper.GetEnumType(enumName);
                    if (!Enum.IsDefined(enumType, enumValue))
                        enumValue = ((Enum)Enum.GetValues(enumType).GetValue(0)).ToString();
                    val = Enum.Parse(enumType, enumValue) as Enum;
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