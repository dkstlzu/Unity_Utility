using System;
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
        [Tooltip("Replace old one with new one if duplicate")]
        public bool ReplaceAsNew = false;
        public Component TargetComponent;
        public string HashID;
        void Awake()
        {
            switch (Uniqueness)
            {
                case DontDestroyMethod.Normal :
                Add(TargetComponent, gameObject);
                break;
                case DontDestroyMethod.Unique :
                AddUnique(TargetComponent, gameObject);
                break;
                case DontDestroyMethod.UniqueID :
                if (HashID == "") HashID = transform.name;
                AddUniqueID(TargetComponent, HashID, gameObject);
                break;
            }
        }

        public static List<Component> ComponentList = new List<Component>();
        public static HashSet<Type> ComponentTypeSet = new HashSet<Type>();
        public static HashSet<string> HashIDSet = new HashSet<string>();

        public static int Count
        {
            get{return ComponentList.Count + ComponentTypeSet.Count + HashIDSet.Count;}
        }

        public static bool isEmpty
        {
            get{return ComponentList.Count + ComponentTypeSet.Count + HashIDSet.Count == 0;}
        }

        public static void Add(Component component, GameObject rootObj = null)
        {
            ComponentList.Add(component);
            if (rootObj == null)
            {
                DontDestroyOnLoad(component);
            } else
            {
                DontDestroyOnLoad(rootObj);
            }
        }

        public static void AddUnique(Component component, GameObject rootObj = null)
        {
            if (ComponentTypeSet.Add(component.GetType()))
            {
                if (rootObj == null)
                {
                    DontDestroyOnLoad(component);
                } else
                {
                    DontDestroyOnLoad(rootObj);
                }
            } else
            {
                Destroy(component.gameObject);
            }
        }

        public static void AddUniqueID(Component component, string hashID, GameObject rootObj = null)
        {
            if (HashIDSet.Add(hashID))
            {
                if (rootObj == null)
                {
                    DontDestroyOnLoad(component);
                } else
                {
                    DontDestroyOnLoad(rootObj);
                }
            } else
            {
                Destroy(component.gameObject);
            }
        }

        public static void Remove(Component component)
        {
            ComponentList.Remove(component);
            Destroy(component);
        }

        public static void RemoveUnique(Component component)
        {
            ComponentTypeSet.Remove(component.GetType());
            Destroy(component);
        }

        public static bool Contain(Component component)
        {
            return ComponentList.Contains(component);
        }

        public static bool ContainUnique(Component component)
        {
            return ComponentTypeSet.Contains(component.GetType());
        }
    }
}