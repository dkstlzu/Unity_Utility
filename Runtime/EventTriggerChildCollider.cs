using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace dkstlzu.Utility
{
    [ExecuteInEditMode]
    public class EventTriggerChildCollider : MonoBehaviour
    {
        public EventTrigger Parent;
        public Collider Collider;
        public Collider2D Collider2D;
        public Object ValidCollider
        {
            get 
            {
                if (Use2D) return Collider2D;
                else return Collider;
            }
        }
        internal EventTrigger.EventTriggerState State;

        public bool Use2D => Parent.Use2D;

        void Reset()
        {
            Parent = GetComponentInParent<EventTrigger>();
            if (!Parent)
            {
                var headertext = "Warning";
                var maintext = "Could not Find EventTrigger among parents";
                var ops1 = "Ok";

#if UNITY_EDITOR
                if (EditorUtility.DisplayDialog(headertext, maintext, ops1)) {
                    Debug.LogWarning("Check if EventTrigger exist.");
                } 
#endif

                Debug.LogWarning("Check if EventTrigger exist.");
                DestroyImmediate(this);
                return;
            }

            if (Use2D)
            {
                Collider2D = gameObject.AddComponent(Parent.Collider2D.GetType()) as Collider2D;
                Collider2D!.isTrigger = true;
            } else
            {
                Collider = gameObject.AddComponent(Parent.Collider.GetType()) as Collider;
                Collider!.isTrigger = true;
            }
            
            Parent.Children.Add(this);
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
            Parent.Children.Remove(this);
        }
    }
}