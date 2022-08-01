using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dkstlzu.Utility
{
    [DefaultExecutionOrder(-100)]
    public class UniqueComponent : MonoBehaviour
    {

        public enum UniqueComponentMethod
        {
            Normal,
            Unique,
            UniqueID,
        }

        public UniqueComponentMethod Uniqueness;
        public Component TargetComponent;
        public string HashID;
        public GameObject DestroyGameObject;
        [Tooltip("Replace old one with new one if duplicate")]
        public bool ReplacePreviousOne = false;
        public bool DestroyRequiredComponentsAlso;

        static Dictionary<object, Component> ComponentsDict = new Dictionary<object, Component>();

        void Awake()
        {
            if (!_Add())
            {
                if (DestroyGameObject) DestroyImmediate(DestroyGameObject);
                else if (TargetComponent is Transform || TargetComponent is RectTransform) DestroyImmediate(TargetComponent.gameObject);
                else if (!DestroyRequiredComponentsAlso) DestroyImmediate(TargetComponent);
                else if (DestroyRequiredComponentsAlso) DestroyWithRequireComponent(TargetComponent);

                Destroy(this);
            }

            // isRegistered = add();
            
            // if (isRegistered)
            // {
            //     StartCoroutine(UniqueAwakeInvoker());
            // } else
            // {
            //     DestroyWithRequireComponent();
            // }
        }

        void Update()
        {

        }

        public void DestroyTarget()
        {
            StartCoroutine(UniqueOnDestroyInvoker());
        }

        IEnumerator UniqueAwakeInvoker()
        {
            yield return null;
            if (TargetComponent)
                TargetComponent.SendMessage("UniqueAwake", SendMessageOptions.DontRequireReceiver);
        }

        IEnumerator UniqueOnDestroyInvoker()
        {
            print("UniqueDestroyDB1");
            if (TargetComponent)
                TargetComponent.SendMessage("UniqueOnDestroy", SendMessageOptions.DontRequireReceiver);
            yield return null;

            DestroyWithRequireComponent(TargetComponent);
        }

        void DestroyWithRequireComponent(Component component)
        {
            if (DestroyGameObject) Destroy(DestroyGameObject);
            else 
            {
                if (component is Transform || component is RectTransform)
                    Destroy(component.gameObject);
                else
                {
                    Component component0 = null, component1 = null, component2 = null;
                    if (component.GetType().IsDefined(typeof(RequireComponent), true))
                    {
                        RequireComponent RC = (RequireComponent) Attribute.GetCustomAttribute(component.GetType(), typeof(RequireComponent));

                        if (RC.m_Type0 != null) component0 = component.GetComponent(RC.m_Type0);
                        if (RC.m_Type1 != null) component1 = component.GetComponent(RC.m_Type1);
                        if (RC.m_Type2 != null) component2 = component.GetComponent(RC.m_Type2);
                    }

                    if (component) 
                    {
                        Destroy(component);
                    }
                    if (component0 != null) Destroy(component0);
                    if (component1 != null) Destroy(component1);
                    if (component2 != null) Destroy(component2);
                }
            }
        }

        private bool _Add()
        {
            return Add(TargetComponent, Uniqueness, HashID, ReplacePreviousOne);
        }

        public static bool Add(Component component, UniqueComponentMethod method, string HashID, bool resplacePrevious = false)
        {
            switch (method)
            {
                case UniqueComponentMethod.Normal:
                return AddNormal(component, resplacePrevious);
                case UniqueComponentMethod.Unique:
                return AddUnique(component, resplacePrevious);
                case UniqueComponentMethod.UniqueID:
                if (HashID == string.Empty) Debug.LogError($"{component.gameObject}'s DDM has wrong Unique ID");
                return AddUniqueID(component, HashID, resplacePrevious);
                default: return false;
            }
        }

        static bool AddNormal(Component component, bool resplacePrevious = false)
        {
            ComponentsDict.Add(component.GetHashCode(), component);
            return true;
        }

        static bool AddUnique(Component component, bool resplacePrevious = false)
        {
            bool result = false;

            if (ComponentsDict.ContainsKey(component.GetType()))
            {
                if (resplacePrevious)
                {
                    ComponentsDict[component.GetType()].GetComponent<UniqueComponent>().DestroyTarget();
                    ComponentsDict[component.GetType()] = component;
                    result = true;
                } else
                {
                    result = false;
                }
            } else
            {
                ComponentsDict[component.GetType()] = component;
                result = true;
            }

            return result;
        }

        static bool AddUniqueID(Component component, string hashID, bool resplacePrevious = false)
        {
            bool result = false;

            if (ComponentsDict.ContainsKey(hashID))
            {
                if (resplacePrevious)
                {
                    ComponentsDict[hashID].GetComponent<UniqueComponent>().DestroyTarget();
                    ComponentsDict[hashID] = component;
                    result = true;
                } else
                {
                    result = false;
                }
            } else
            {
                ComponentsDict[hashID] = component;
                result = true;
            }

            return result;
        }
    }
}