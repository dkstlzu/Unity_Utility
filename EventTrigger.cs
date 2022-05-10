#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Utility
{
    public class EventTrigger : MonoBehaviour
    {
        public enum EventTriggerState
        {
            Default,
            Staying,
        }

        public bool use2D;
        public bool isReady;
        public LayerMask TargetLayerMask;
        public bool PlayOnlyFirst;
        private bool enteredOnce;
        private bool exitedOnce;
        public UnityEvent OnTriggerEnterEvent = new UnityEvent();
        public UnityEvent OnTriggerStayEvent = new UnityEvent();
        public UnityEvent OnTriggerExitEvent = new UnityEvent();
        public Collider Collider;
        public Collider2D Collider2D;
        public Object ValidCollider
        {
            get {if (use2D) return Collider2D; else return Collider;}
        }

        internal EventTriggerState State;
        internal List<EventTriggerChildCollider> Childs = new List<EventTriggerChildCollider>();
        internal bool noOneEntered
        {
            get 
            {
                bool result = true;
                result = !(State == EventTriggerState.Staying);
                if (!result) return false;

                foreach (EventTriggerChildCollider ETChild in Childs)
                {
                    result = !(ETChild.State == EventTriggerState.Staying);
                    if (!result) return false;
                }

                return true;
            }
        }

        internal bool stayCalledAlready;

        void FixedUpdate()
        {
            stayCalledAlready = false;
        }

        internal void OnTriggerEnter(Collider collider)
        {
            if ((enteredOnce && PlayOnlyFirst) || use2D || ((1 << collider.gameObject.layer) & TargetLayerMask.value) == 0)
            {
                return;
            }
            
            if (noOneEntered)
            {
                OnTriggerEnterEvent.Invoke();
                enteredOnce = true;
            }

            State = EventTriggerState.Staying;
        }

        internal void OnTriggerStay(Collider collider)
        {
            if (use2D || ((1 << collider.gameObject.layer) & TargetLayerMask.value) == 0)
            {
                return;
            }
            
            if (!stayCalledAlready)
            {
                OnTriggerStayEvent.Invoke();
            }

            stayCalledAlready = true;
        }

        internal void OnTriggerExit(Collider collider)
        {
            if ((exitedOnce && PlayOnlyFirst) || use2D || ((1 << collider.gameObject.layer) & TargetLayerMask.value) == 0)
            {
                return;
            }
            
            State = EventTriggerState.Default;

            if (noOneEntered)
            {
                OnTriggerExitEvent.Invoke();
                exitedOnce = true;
            }
        }

        internal void OnTriggerEnter2D(Collider2D collider)
        {
            if ((enteredOnce && PlayOnlyFirst) || !use2D || ((1 << collider.gameObject.layer) & TargetLayerMask.value) == 0)
            {
                return;
            }
            
            if (noOneEntered)
            {
                OnTriggerEnterEvent.Invoke();
                enteredOnce = true;
            }
            
            State = EventTriggerState.Staying;
        }

        internal void OnTriggerStay2D(Collider2D collider)
        {
            if (!use2D || ((1 << collider.gameObject.layer) & TargetLayerMask.value) == 0)
            {
                return;
            }

            if (!stayCalledAlready)
            {
                OnTriggerStayEvent.Invoke();
            }

            stayCalledAlready = true;
        }

        internal void OnTriggerExit2D(Collider2D collider)
        {
            if ((exitedOnce && PlayOnlyFirst) || !use2D || ((1 << collider.gameObject.layer) & TargetLayerMask.value) == 0)
            {
                return;
            }
            
            State = EventTriggerState.Default;

            if (noOneEntered)
            {
                OnTriggerExitEvent.Invoke();
                exitedOnce = true;
            }
        }

        public void ReuseEvent()
        {
            enteredOnce = false;
            exitedOnce = false;
        }

#if UNITY_EDITOR
        public void AddEnterEvent(UnityAction action)
        {
            UnityEventTools.AddPersistentListener(OnTriggerEnterEvent, action);
        }

        public void AddStayEvent(UnityAction action)
        {
            UnityEventTools.AddPersistentListener(OnTriggerStayEvent, action);
        }

        public void AddExitEvent(UnityAction action)
        {
            UnityEventTools.AddPersistentListener(OnTriggerExitEvent, action);
        }
#endif
    }
}