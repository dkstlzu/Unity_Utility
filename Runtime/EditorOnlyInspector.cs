using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public class EditorOnlyInspector : MonoBehaviour
{
    [SerializeReference]
    public object obj;

    private MethodInfo gizmoMethod;
    private MethodInfo gizmoSelectedMethod;
    
    private void Awake()
    {
#if !UNITY_EDITOR
        Destroy(this);
#endif
    }

    private void SetValue(object obj)
    {
        this.obj = obj;
        
        gizmoMethod = obj.GetType()?.GetMethod("OnDrawGizmos", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        gizmoSelectedMethod = obj.GetType()?.GetMethod("OnDrawGizmosSelected", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);   
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        gizmoMethod?.Invoke(obj, null);
    }
    
    void OnDrawGizmosSelected()
    {
        gizmoSelectedMethod?.Invoke(obj, null);
    }
#endif


    [Conditional("UNITY_EDITOR")]
    public static void AddOn(GameObject gameObject, object obj)
    {
        gameObject.AddComponent<EditorOnlyInspector>().SetValue(obj);
    }
}
