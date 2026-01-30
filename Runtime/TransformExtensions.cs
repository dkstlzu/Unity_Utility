using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace dkstlzu.Utility
{
    public static class TransformExtensions
    {
        public static void TraverseDescendents(this Transform transform, Action<Transform> action, Func<Transform, bool> breakAction = null)
        {
            if (breakAction != null)
            {
                if (breakAction(transform))
                {
                    return;
                }
            }
            action?.Invoke(transform);
    
            foreach (Transform child in transform)
            {
                child.TraverseDescendents(action, breakAction);
            }
        }
    
        public static void TraverseDescendentsWithDepth(this Transform transform, Action<Transform, int> action)
        {
            int depth = 0;
            TraverseDescendentsWithDepth_Recursive(transform, action, depth);
        }
    
        private static void TraverseDescendentsWithDepth_Recursive(this Transform transform, Action<Transform, int> action, int depth)
        {
            action?.Invoke(transform, depth);
    
            depth++;
            foreach (Transform child in transform)
            {
                child.TraverseDescendentsWithDepth_Recursive(action, depth);
            }
        }
    
        public static void TraverseParents(this Transform transform, Action<Transform> action, Func<Transform, bool> breakAction = null)
        {
            if (breakAction != null)
            {
                if (breakAction(transform))
                {
                    return;
                }
            }
            action?.Invoke(transform);

            if (transform.parent != null)
            {
                transform.parent.TraverseParents(action, breakAction);
            }
        }
        
        public static void TraverseParentsWithDepth(this Transform transform, Action<Transform, int> action)
        {
            int depth = 0;
            TraverseParentsWithDepth_Recursive(transform, action, depth);
        }

        private static void TraverseParentsWithDepth_Recursive(this Transform transform, Action<Transform, int> action, int depth)
        {
            action?.Invoke(transform, depth);

            depth++;
            foreach (Transform child in transform)
            {
                child.TraverseDescendentsWithDepth_Recursive(action, depth);
            }
        }
        
        public static string PrintChildren(this Transform transform)
        {
            StringBuilder stringBuilder = new StringBuilder();
    
            transform.TraverseDescendentsWithDepth((child, depth) =>
            {
                stringBuilder.Append("  ".Multiply(depth));
                stringBuilder.AppendLine($"{child} : {child.gameObject.activeSelf}");
            });
            
            return stringBuilder.ToString();
        }
        
        public static string UpToRootName(this Transform transform)
        {
            StringBuilder builder = new StringBuilder();

            while (transform != null)
            {
                if (builder.Length > 0)
                {
                    builder.Insert(0, transform.name + "--");
                }
                else
                {
                    builder.Append(transform.name);
                }
                transform = transform.parent;
            }

            builder.AppendLine();
            return builder.ToString();
        }
        
        public static void PrintUpToRoot(this Transform transform)
        {
            StringBuilder builder = new StringBuilder();

            while (transform != null)
            {
                builder.Insert(0, transform.name + "\n");
                transform = transform.parent;
            }
            
            Debug.Log(builder.ToString());
        }

        public static Transform RootTransform(this Transform transform)
        {
            while (transform.parent != null)
            {
                transform = transform.parent;
            }

            return transform;
        }
        
        public static RectTransform GetRectTransform(this Transform transform) => transform as RectTransform;
        public static RectTransform GetRectTransform(this GameObject gameObject) => gameObject.transform as RectTransform;
        public static RectTransform GetRectTransform(this Component component) => component.transform as RectTransform;

        public static T GetComponentInParentOnly<T>(this Transform transform, bool includeInactive = false) where T : Component
        {
            if (!transform.parent)
            {
                return null;
            }

            return transform.parent.GetComponentInParent<T>(includeInactive);
        }
        
        public static T[] GetComponentsInParentOnly<T>(this Transform transform) where T : Component
        {
            if (!transform.parent)
            {
                return Array.Empty<T>();
            }

            return transform.parent.GetComponentsInParent<T>();
        }
        
        public static T[] GetComponentsInParentOnly<T>(this Transform transform, bool includeInactive) where T : Component
        {
            if (!transform.parent)
            {
                return Array.Empty<T>();
            }

            return transform.parent.GetComponentsInParent<T>(includeInactive);
        }
        
        public static void GetComponentsInParentOnly<T>(this Transform transform, bool includeInactive, List<T> results) where T : Component
        {
            transform.parent.GetComponentsInParent<T>(includeInactive, results);
        }

        public static T GetComponentInChildrenOnly<T>(this Transform transform, bool includeInactive = false) where T : Component
        {
            if (transform.childCount == 0)
            {
                return null;
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                T component = transform.GetChild(i).GetComponentInChildren<T>(includeInactive);
                if (component)
                {
                    return component;
                }
            }

            return null;
        }
        
        public static T[] GetComponentsInChildrenOnly<T>(this Transform transform) where T : Component
        {
            if (transform.childCount == 0)
            {
                return Array.Empty<T>();
            }

            List<T> results = new List<T>();
            
            for (int i = 0; i < transform.childCount; i++)
            {
                results.AddRange(transform.GetChild(i).GetComponentsInChildren<T>());
            }

            return results.ToArray();
        }
        
        public static T[] GetComponentsInChildrenOnly<T>(this Transform transform, bool includeInactive) where T : Component
        {
            if (transform.childCount == 0)
            {
                return Array.Empty<T>();
            }

            List<T> results = new List<T>();
            
            for (int i = 0; i < transform.childCount; i++)
            {
                results.AddRange(transform.GetChild(i).GetComponentsInChildren<T>(includeInactive));
            }

            return results.ToArray();
        }
        
        public static T[] GetComponentsInChildrenOnly<T>(this Transform transform, bool includeInactive, List<T> results) where T : Component
        {
            results.Clear();
            
            if (transform.childCount == 0)
            {
                return Array.Empty<T>();
            }
            
            for (int i = 0; i < transform.childCount; i++)
            {
                results.AddRange(transform.GetChild(i).GetComponentsInChildren<T>(includeInactive));
            }

            return results.ToArray();
        }
        
        public static T GetComponentInParentOnly<T>(this GameObject gameObject, bool includeInactive = false) where T : Component => gameObject.transform.GetComponentInParentOnly<T>(includeInactive);
        public static T[] GetComponentsInParentOnly<T>(this GameObject gameObject) where T : Component => gameObject.transform.GetComponentsInParentOnly<T>();
        public static T[] GetComponentsInParentOnly<T>(this GameObject gameObject, bool includeInactive) where T : Component => gameObject.transform.GetComponentsInParentOnly<T>(includeInactive);
        public static void GetComponentsInParentOnly<T>(this GameObject gameObject, bool includeInactive, List<T> results) where T : Component => gameObject.transform.GetComponentsInParentOnly<T>(includeInactive, results);
        public static T GetComponentInChildrenOnly<T>(this GameObject gameObject, bool includeInactive = false) where T : Component => gameObject.transform.GetComponentInChildrenOnly<T>(includeInactive);
        public static T[] GetComponentsInChildrenOnly<T>(this GameObject gameObject) where T : Component => gameObject.transform.GetComponentsInChildrenOnly<T>();
        public static T[] GetComponentsInChildrenOnly<T>(this GameObject gameObject, bool includeInactive) where T : Component => gameObject.transform.GetComponentsInChildrenOnly<T>(includeInactive);
        public static T[] GetComponentsInChildrenOnly<T>(this GameObject gameObject, bool includeInactive, List<T> results) where T : Component => gameObject.transform.GetComponentsInChildrenOnly<T>(includeInactive, results);
        
        public static T GetComponentInParentOnly<T>(this Component component, bool includeInactive = false) where T : Component => component.transform.GetComponentInParentOnly<T>(includeInactive);
        public static T[] GetComponentsInParentOnly<T>(this Component component) where T : Component => component.transform.GetComponentsInParentOnly<T>();
        public static T[] GetComponentsInParentOnly<T>(this Component component, bool includeInactive) where T : Component => component.transform.GetComponentsInParentOnly<T>(includeInactive);
        public static void GetComponentsInParentOnly<T>(this Component component, bool includeInactive, List<T> results) where T : Component => component.transform.GetComponentsInParentOnly<T>(includeInactive, results);
        public static T GetComponentInChildrenOnly<T>(this Component component, bool includeInactive = false) where T : Component => component.transform.GetComponentInChildrenOnly<T>(includeInactive);
        public static T[] GetComponentsInChildrenOnly<T>(this Component component) where T : Component => component.transform.GetComponentsInChildrenOnly<T>();
        public static T[] GetComponentsInChildrenOnly<T>(this Component component, bool includeInactive) where T : Component => component.transform.GetComponentsInChildrenOnly<T>(includeInactive);
        public static T[] GetComponentsInChildrenOnly<T>(this Component component, bool includeInactive, List<T> results) where T : Component => component.transform.GetComponentsInChildrenOnly<T>(includeInactive, results);
    }
    
    public static class StringExtensions
    {
        public static string Multiply(this string source, int multiplier)
        {
            StringBuilder sb = new StringBuilder(multiplier * source.Length);
            for (int i = 0; i < multiplier; i++)
            {
                sb.Append(source);
            }
    
            return sb.ToString();
        }
    }
}
