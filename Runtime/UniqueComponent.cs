using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace dkstlzu.Utility
{
    [DefaultExecutionOrder(-100)]
    public class UniqueComponent : MonoBehaviour
    {
        public enum UniqueComponentMethod
        {
            UniqueType,
            UniqueID,
        }

        public UniqueComponentMethod Uniqueness;
        public Component TargetComponent;
        public string HashID;
        
        [Tooltip("Replace old one with new one if duplicate")]
        public bool ReplacePreviousOne = false;

        static Dictionary<string, Component> _componentsDict = new Dictionary<string, Component>();

        void Awake()
        {
            if (!Add(TargetComponent, Uniqueness, HashID, ReplacePreviousOne))
            {
                Destroy(this);
            }
        }

        public static bool Add(Component component, UniqueComponentMethod method, string hashID, bool replacePrevious = false)
        {
            switch (method)
            {
                default:
                case UniqueComponentMethod.UniqueType:
                return AddUniqueType(component, replacePrevious);
                case UniqueComponentMethod.UniqueID:
                return AddUniqueID(component, hashID, replacePrevious);
            }
        }
        
        public static bool AddUniqueType(Component component, bool replacePrevious = false)
        {
            return AddUniqueID(component, component.GetType().FullName, replacePrevious);
        }

        public static bool AddUniqueID(Component component, string hashID, bool replacePrevious = false)
        {
            Assert.IsNotNull(hashID);
            Assert.AreNotEqual(string.Empty, hashID);
            
            if (_componentsDict.TryGetValue(hashID, out Component previous))
            {
                if (replacePrevious)
                {
                    DestroyTarget(previous);
                    _componentsDict[hashID] = component;
                }
                else
                {
                    DestroyTarget(component);   
                }
                
                return replacePrevious;
            }
            
            _componentsDict.Add(hashID, component);
            return true;
        }

        public static void DestroyTarget(Component component)
        {
            if (component != null)
            {
                if (component is Transform || component is RectTransform) DestroyImmediate(component.gameObject);
                else DestroyImmediate(component);
            }
        }
    }
}