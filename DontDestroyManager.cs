﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

namespace Utility
{
    public class DontDestroyManager : MonoBehaviour
    {
        public enum DontDestroyMethod
        {
            Normal,
            Unique,
            UniqueID,
        }

        public DontDestroyMethod Uniqueness;
        public Component TargetComponent;
        public string HashID;
        [Tooltip("Replace old one with new one if duplicate")]
        public bool ReplacePreviousOne = false;
        void Awake()
        {
            Add(TargetComponent, Uniqueness, HashID, ReplacePreviousOne);
            Destroy(this);
        }

        static Dictionary<object, Component> ComponentsDict = new Dictionary<object, Component>();

        public static int Count
        {
            get{return ComponentsDict.Count;}
        }

        public static bool isEmpty
        {
            get{return ComponentsDict.Count == 0;}
        }

        public static void Add(Component component, DontDestroyMethod method, string HashID, bool resplacePrevious = false, GameObject rootObj = null)
        {
            switch (method)
            {
                case DontDestroyMethod.Normal:
                AddNormal(component, resplacePrevious, rootObj);
                break;
                case DontDestroyMethod.Unique:
                Unique:
                AddUnique(component, resplacePrevious, rootObj);
                break;
                case DontDestroyMethod.UniqueID:
                if (HashID == string.Empty) goto Unique;
                AddUniqueID(component, HashID, resplacePrevious, rootObj);
                break;
            }
        }

        static void AddNormal(Component component, bool resplacePrevious = false, GameObject rootObj = null)
        {
            ComponentsDict.Add(component.GetHashCode(), component);
            if (rootObj == null)
            {
                DontDestroyOnLoad(component);
            }
        }

        static void AddUnique(Component component, bool resplacePrevious = false, GameObject rootObj = null)
        {
            if (Contain(component.GetType()))
            {
                if (resplacePrevious)
                {
                    Destroy(ComponentsDict[component.GetType()]);
                    ComponentsDict[component.GetType()] = component;
                    DontDestroyOnLoad(component);
                } else
                {
                    Destroy(component);
                }
            } else
            {
                ComponentsDict[component.GetType()] = component;
                DontDestroyOnLoad(component);
            }
        }

        static void AddUniqueID(Component component, string hashID, bool resplacePrevious = false, GameObject rootObj = null)
        {
            if (Contain(hashID))
            {
                if (resplacePrevious)
                {
                    if (component is Transform || component is RectTransform)
                    {
                        Destroy(ComponentsDict[hashID].gameObject);
                    } else
                    {
                        Destroy(ComponentsDict[hashID]);
                    }
                    ComponentsDict[hashID] = component;
                    DontDestroyOnLoad(component);
                } else
                {
                    if (component is Transform || component is RectTransform)
                    {
                        Destroy(component.gameObject);
                    } else
                    {
                        Destroy(component);
                    }
                }
            } else
            {
                ComponentsDict[hashID] = component;
                DontDestroyOnLoad(component);
            }
        }

        public static void Remove(int hasdCode)
        {
            ComponentsDict.Remove(hasdCode);
        }

        public static void Remove(Type uniqueType)
        {
            ComponentsDict.Remove(uniqueType);
        }

        public static void Remove(string hashID)
        {
            ComponentsDict.Remove(hashID);
        }

        public static bool Contain(Component component)
        {
            return ComponentsDict.ContainsValue(component);
        }

        public static bool Contain(int hasdCode)
        {
            return ComponentsDict.ContainsKey(hasdCode);
        }

        public static bool Contain(Type uniqueType)
        {
            return ComponentsDict.ContainsKey(uniqueType);
        }

        public static bool Contain(string hashID)
        {
            return ComponentsDict.ContainsKey(hashID);
        }
    }
}