#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace dkstlzu.Utility
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
#if UNITY_2020_1_OR_NEWER
        public UnityEvent<GameObject> OnTriggerEnterGOEvent = new UnityEvent<GameObject>();
        public UnityEvent<GameObject> OnTriggerStayGOEvent = new UnityEvent<GameObject>();
        public UnityEvent<GameObject> OnTriggerExitGOEvent = new UnityEvent<GameObject>();
#else
        [System.Serializable]
        public class GameObjectUnityEvent : UnityEvent<GameObject> {}
        public GameObjectUnityEvent OnTriggerEnterGOEvent = new GameObjectUnityEvent();
        public GameObjectUnityEvent OnTriggerStayGOEvent = new GameObjectUnityEvent();
        public GameObjectUnityEvent OnTriggerExitGOEvent = new GameObjectUnityEvent();
#endif
        public Collider Collider;
        public Collider2D Collider2D;
        public Object ValidCollider
        {
            get {if (use2D) return Collider2D; else return Collider;}
        }

        internal EventTriggerState State;
        public List<EventTriggerChildCollider> Childs = new List<EventTriggerChildCollider>();
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
                OnTriggerEnterEvent?.Invoke();
                OnTriggerExitGOEvent?.Invoke(collider.gameObject);
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
                OnTriggerStayEvent?.Invoke();
                OnTriggerStayGOEvent?.Invoke(collider.gameObject);
                stayCalledAlready = true;
            }

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
                OnTriggerExitEvent?.Invoke();
                OnTriggerExitGOEvent?.Invoke(collider.gameObject);
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
                OnTriggerEnterEvent?.Invoke();
                OnTriggerEnterGOEvent?.Invoke(collider.gameObject);
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
                OnTriggerStayEvent?.Invoke();
                OnTriggerStayGOEvent?.Invoke(collider.gameObject);
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
                OnTriggerExitEvent?.Invoke();
                OnTriggerExitGOEvent?.Invoke(collider.gameObject);
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

        public void RemoveEnterEvent(UnityAction action)
        {
            UnityEventTools.RemovePersistentListener(OnTriggerEnterEvent, action);
        }

        public void RemoveStayEvent(UnityAction action)
        {
            UnityEventTools.RemovePersistentListener(OnTriggerStayEvent, action);
        }

        public void RemoveExitEvent(UnityAction action)
        {
            UnityEventTools.RemovePersistentListener(OnTriggerExitEvent, action);
        }

        public void AddEnterGOEvent(UnityAction<GameObject> action)
        {
            UnityEventTools.AddPersistentListener(OnTriggerEnterGOEvent, action);
        }

        public void AddStayGOEvent(UnityAction<GameObject> action)
        {
            UnityEventTools.AddPersistentListener(OnTriggerStayGOEvent, action);
        }

        public void AddExitGOEvent(UnityAction<GameObject> action)
        {
            UnityEventTools.AddPersistentListener(OnTriggerExitGOEvent, action);
        }

        public void RemoveEnterGOEvent(UnityAction<GameObject> action)
        {
            UnityEventTools.RemovePersistentListener(OnTriggerEnterGOEvent, action);
        }

        public void RemoveStayGOEvent(UnityAction<GameObject> action)
        {
            UnityEventTools.RemovePersistentListener(OnTriggerStayGOEvent, action);
        }

        public void RemoveExitGOEvent(UnityAction<GameObject> action)
        {
            UnityEventTools.RemovePersistentListener(OnTriggerExitGOEvent, action);
        }

        public void ClearEnterEvent()
        {
            int eventNum = OnTriggerEnterEvent.GetPersistentEventCount();

            while (eventNum-- > 0)
            {
                UnityEventTools.RemovePersistentListener(OnTriggerEnterEvent, 0);
            }

            eventNum = OnTriggerEnterGOEvent.GetPersistentEventCount();

            while (eventNum-- > 0)
            {
                UnityEventTools.RemovePersistentListener(OnTriggerEnterGOEvent, 0);
            }
        }

        public void ClearStayEvent()
        {
            int eventNum = OnTriggerStayEvent.GetPersistentEventCount();

            while (eventNum-- > 0)
            {
                UnityEventTools.RemovePersistentListener(OnTriggerStayEvent, 0);
            }

            eventNum = OnTriggerStayGOEvent.GetPersistentEventCount();

            while (eventNum-- > 0)
            {
                UnityEventTools.RemovePersistentListener(OnTriggerStayGOEvent, 0);
            }
        }

        public void ClearExitEvent()
        {
            int eventNum = OnTriggerExitEvent.GetPersistentEventCount();

            while (eventNum-- > 0)
            {
                UnityEventTools.RemovePersistentListener(OnTriggerExitEvent, 0);
            }

            eventNum = OnTriggerExitGOEvent.GetPersistentEventCount();

            while (eventNum-- > 0)
            {
                UnityEventTools.RemovePersistentListener(OnTriggerExitGOEvent, 0);
            }
        }
#endif
    }
}