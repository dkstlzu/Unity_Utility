using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace dkstlzu.Utility
{
    [CustomEditor(typeof(EventTriggerChildCollider))]
    public class EventTriggerChildColliderEditor : ColliderEditor
    {
        protected override void OnEnable()
        {
            IsReadyName = "_isReady";
            base.OnEnable();
        }
    }
}