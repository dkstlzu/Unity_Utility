using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utility
{
    [ExecuteInEditMode]
    public class EventTriggerChildCollider : MonoBehaviour
    {
        public EventTrigger ET;
        public Collider Collider;
        public Collider2D Collider2D;
        public Object ValidCollider
        {
            get 
            {
                if (use2D) return Collider2D;
                else return Collider;
            }
        }
        internal EventTrigger.EventTriggerState State;

        public bool use2D
        {
            get {return ET.use2D;}
        }

        void Reset()
        {
            ET = GetComponentInParent<EventTrigger>();
            if (!ET)
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

            if (use2D)
            {
                Collider2D = gameObject.AddComponent(ET.Collider2D.GetType()) as Collider2D;
                Collider2D.isTrigger = true;
            } else
            {
                Collider = gameObject.AddComponent(ET.Collider.GetType()) as Collider;
                Collider.isTrigger = true;
            }
            ET.Childs.Add(this);
        }

        void OnTriggerEnter(Collider collider)
        {
            ET.OnTriggerEnter(collider);
            State = EventTrigger.EventTriggerState.Staying;
        }

        void OnTriggerStay(Collider collider)
        {
            ET.OnTriggerStay(collider);
        }

        void OnTriggerExit(Collider collider)
        {
            State = EventTrigger.EventTriggerState.Default;
            ET.OnTriggerExit(collider);
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            ET.OnTriggerEnter2D(collider);
            State = EventTrigger.EventTriggerState.Staying;
        }

        void OnTriggerStay2D(Collider2D collider)
        {
            ET.OnTriggerStay2D(collider);
        }

        void OnTriggerExit2D(Collider2D collider)
        {
            State = EventTrigger.EventTriggerState.Default;
            ET.OnTriggerExit2D(collider);
        }

        void OnDestroy()
        {
            ET.Childs.Remove(this);
        }
    }
}