using System;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace dkstlzu.Utility
{
    public class EventTriggerChildCollider : MonoBehaviour
    {
        public EventTrigger Parent { get; private set; }
        public bool Use2D => _collider.GetType().IsAssignableFrom(typeof(Collider2D));

        [SerializeField]
        private bool _isReady;
        public bool IsReady => (Parent?.IsReady ?? false) && _isReady;
        
        [SerializeField]
        private Object _collider;
        
        internal EventTrigger.EventTriggerState State;

        private void Awake()
        {
            Parent = GetComponentInParent<EventTrigger>();
            if (Parent)
            {
                Parent.Children.Add(this);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            Parent.OnTriggerEnter(other);
            State = EventTrigger.EventTriggerState.Staying;
        }

        void OnTriggerStay(Collider other)
        {
            Parent.OnTriggerStay(other);
        }

        void OnTriggerExit(Collider other)
        {
            State = EventTrigger.EventTriggerState.Default;
            Parent.OnTriggerExit(other);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            Parent.OnTriggerEnter2D(other);
            State = EventTrigger.EventTriggerState.Staying;
        }

        void OnTriggerStay2D(Collider2D other)
        {
            Parent.OnTriggerStay2D(other);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            State = EventTrigger.EventTriggerState.Default;
            Parent.OnTriggerExit2D(other);
        }

        void OnDestroy()
        {
            Destroy(_collider);
            Parent.Children.Remove(this);
        }
    }
}