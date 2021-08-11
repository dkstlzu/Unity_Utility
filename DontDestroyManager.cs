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
        }

        public DontDestroyMethod HowTo;
        public Component TargetComponent;
        void Awake()
        {
            switch (HowTo)
            {
                case DontDestroyMethod.Normal :
                Add(TargetComponent);
                break;
                case DontDestroyMethod.Unique :
                AddUnique(TargetComponent);
                break;
            }
        }

        public static List<Component> ComponentList = new List<Component>();
        public static HashSet<Type> ComponentTypeSet = new HashSet<Type>();

        public static int Count
        {
            get{return ComponentList.Count + ComponentTypeSet.Count;}
        }

        public static bool isEmpty
        {
            get{return ComponentList.Count + ComponentTypeSet.Count == 0;}
        }

        public static void Add(Component component)
        {
            ComponentList.Add(component);
            DontDestroyOnLoad(component);
        }

        public static void AddUnique(Component component)
        {
            if (ComponentTypeSet.Add(component.GetType()))
            {
                DontDestroyOnLoad(component);
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