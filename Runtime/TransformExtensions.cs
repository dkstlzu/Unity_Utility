using System;
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
