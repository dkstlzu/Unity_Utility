using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Utility
{
    public class EventTrigger : MonoBehaviour
    {
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

        void OnTriggerEnter(Collider collider)
        {
            if ((enteredOnce && PlayOnlyFirst) || use2D)
            {
                return;
            }
            
            if (1 << collider.gameObject.layer == TargetLayerMask.value)
            {
                OnTriggerEnterEvent.Invoke();
                enteredOnce = true;
            }
        }

        void OnTriggerStay(Collider collider)
        {
            if (use2D)
            {
                return;
            }
            
            if (1 << collider.gameObject.layer == TargetLayerMask.value)
            {
                OnTriggerStayEvent.Invoke();
            }
        }

        void OnTriggerExit(Collider collider)
        {
            if ((exitedOnce && PlayOnlyFirst) || use2D)
            {
                return;
            }
            
            if (1 << collider.gameObject.layer == TargetLayerMask.value)
            {
                OnTriggerExitEvent.Invoke();
                exitedOnce = true;
            }
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            if ((enteredOnce && PlayOnlyFirst) || !use2D)
            {
                return;
            }
            
            if (1 << collider.gameObject.layer == TargetLayerMask.value)
            {
                OnTriggerEnterEvent.Invoke();
                enteredOnce = true;
            }
        }

        void OnTriggerStay2D(Collider2D collider)
        {
            if (!use2D)
            {
                return;
            }
            
            if (1 << collider.gameObject.layer == TargetLayerMask.value)
            {
                OnTriggerStayEvent.Invoke();
            }
        }

        void OnTriggerExit2D(Collider2D collider)
        {
            if ((exitedOnce && PlayOnlyFirst) || !use2D)
            {
                return;
            }
            
            if (1 << collider.gameObject.layer == TargetLayerMask.value)
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
    }
}