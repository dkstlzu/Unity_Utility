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
        public Component TargetComponent;
        public string HashID;
        public GameObject DestroyGameObject;
        [Tooltip("Replace old one with new one if duplicate")]
        public bool ReplacePreviousOne = false;
        private bool isRegistered = false;
        void Awake()
        {
            isRegistered = add();
            if (isRegistered)
            {
                if (TargetComponent is MonoBehaviour mb)
                {
                    mb.StartCoroutine(DDMAwakeInvoker());
                }
            } else
            {
                DestroyTarget();
            }
        }

        IEnumerator DDMAwakeInvoker()
        {
            yield return null;
            if (TargetComponent)
                TargetComponent.SendMessage("DDMAwake", SendMessageOptions.DontRequireReceiver);
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

        bool add()
        {
            return Add(TargetComponent, Uniqueness, HashID, ReplacePreviousOne, DestroyGameObject);
        }

        public static bool Add(Component component, DontDestroyMethod method, string HashID, bool resplacePrevious = false, GameObject rootObj = null)
        {
            switch (method)
            {
                case DontDestroyMethod.Normal:
                return AddNormal(component, resplacePrevious, rootObj);
                case DontDestroyMethod.Unique:
                return AddUnique(component, resplacePrevious, rootObj);
                case DontDestroyMethod.UniqueID:
                if (HashID == string.Empty) Debug.LogError($"{component.gameObject}'s DDM has wrong Unique ID");
                return AddUniqueID(component, HashID, resplacePrevious, rootObj);
                default: return false;
            }
        }

        static bool AddNormal(Component component, bool resplacePrevious = false, GameObject rootObj = null)
        {
            ComponentsDict.Add(component.GetHashCode(), component);
            DontDestroyOnLoad(component);
            return true;
        }

        static bool AddUnique(Component component, bool resplacePrevious = false, GameObject rootObj = null)
        {
            bool result = false;

            if (Contain(component.GetType()))
            {
                if (resplacePrevious)
                {
                    ComponentsDict[component.GetType()].GetComponent<DontDestroyManager>().DestroyTarget();
                    ComponentsDict[component.GetType()] = component;
                    DontDestroyOnLoad(component);
                    result = true;
                } else
                {
                    result = false;
                }
            } else
            {
                ComponentsDict[component.GetType()] = component;
                DontDestroyOnLoad(component);
                result = true;
            }

            return result;
        }

        static bool AddUniqueID(Component component, string hashID, bool resplacePrevious = false, GameObject rootObj = null)
        {
            bool result = false;

            if (Contain(hashID))
            {
                if (resplacePrevious)
                {
                    ComponentsDict[hashID].GetComponent<DontDestroyManager>().DestroyTarget();
                    ComponentsDict[hashID] = component;
                    DontDestroyOnLoad(component);
                    result = true;
                } else
                {
                    result = false;
                }
            } else
            {
                ComponentsDict[hashID] = component;
                DontDestroyOnLoad(component);
                result = true;
            }

            return result;
        }

        public void DestroyTarget()
        {
            if (DestroyGameObject) Destroy(DestroyGameObject);
            else 
            {
                if (TargetComponent is Transform || TargetComponent is RectTransform)
                    Destroy(TargetComponent.gameObject);
                else
                    Destroy(TargetComponent);
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