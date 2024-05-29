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

        public bool Use2D;
        public bool IsReady;
        public LayerMask TargetLayerMask;
        public bool PlayOnlyFirst;
        private bool _enteredOnce;
        private bool _exitedOnce;
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
            get {if (Use2D) return Collider2D; else return Collider;}
        }

        internal EventTriggerState State;
        public List<EventTriggerChildCollider> Children = new List<EventTriggerChildCollider>();
        internal bool NoOneEntered
        {
            get 
            {
                if (State == EventTriggerState.Staying)
                {
                    return false;
                }

                foreach (EventTriggerChildCollider child in Children)
                {
                    if (child.State == EventTriggerState.Staying)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        internal bool StayCalledAlready;

        void FixedUpdate()
        {
            StayCalledAlready = false;
        }

        internal void OnTriggerEnter(Collider other)
        {
            if ((_enteredOnce && PlayOnlyFirst) || Use2D || ((1 << other.gameObject.layer) & TargetLayerMask.value) == 0)
            {
                return;
            }
            
            if (NoOneEntered)
            {
                OnTriggerEnterEvent?.Invoke();
                OnTriggerExitGOEvent?.Invoke(other.gameObject);
                _enteredOnce = true;
            }

            State = EventTriggerState.Staying;
        }

        internal void OnTriggerStay(Collider other)
        {
            if (Use2D || ((1 << other.gameObject.layer) & TargetLayerMask.value) == 0)
            {
                return;
            }
            
            if (!StayCalledAlready)
            {
                OnTriggerStayEvent?.Invoke();
                OnTriggerStayGOEvent?.Invoke(other.gameObject);
                StayCalledAlready = true;
            }

        }

        internal void OnTriggerExit(Collider other)
        {
            if ((_exitedOnce && PlayOnlyFirst) || Use2D || ((1 << other.gameObject.layer) & TargetLayerMask.value) == 0)
            {
                return;
            }
            
            State = EventTriggerState.Default;

            if (NoOneEntered)
            {
                OnTriggerExitEvent?.Invoke();
                OnTriggerExitGOEvent?.Invoke(other.gameObject);
                _exitedOnce = true;
            }
        }

        internal void OnTriggerEnter2D(Collider2D other)
        {
            if ((_enteredOnce && PlayOnlyFirst) || !Use2D || ((1 << other.gameObject.layer) & TargetLayerMask.value) == 0)
            {
                return;
            }
            
            if (NoOneEntered)
            {
                OnTriggerEnterEvent?.Invoke();
                OnTriggerEnterGOEvent?.Invoke(other.gameObject);
                _enteredOnce = true;
            }
            
            State = EventTriggerState.Staying;
        }

        internal void OnTriggerStay2D(Collider2D other)
        {
            if (!Use2D || ((1 << other.gameObject.layer) & TargetLayerMask.value) == 0)
            {
                return;
            }

            if (!StayCalledAlready)
            {
                OnTriggerStayEvent?.Invoke();
                OnTriggerStayGOEvent?.Invoke(other.gameObject);
            }

            StayCalledAlready = true;
        }

        internal void OnTriggerExit2D(Collider2D other)
        {
            if ((_exitedOnce && PlayOnlyFirst) || !Use2D || ((1 << other.gameObject.layer) & TargetLayerMask.value) == 0)
            {
                return;
            }
            
            State = EventTriggerState.Default;

            if (NoOneEntered)
            {
                OnTriggerExitEvent?.Invoke();
                OnTriggerExitGOEvent?.Invoke(other.gameObject);
                _exitedOnce = true;
            }
        }

        public void ReuseEvent()
        {
            _enteredOnce = false;
            _exitedOnce = false;
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
#else
        // For NonEditor version Declaration of methods
        public void AddEnterEvent(UnityAction action) {OnTriggerEnterEvent.AddListener(action);}
        public void AddStayEvent(UnityAction action) {OnTriggerStayEvent.AddListener(action);}
        public void AddExitEvent(UnityAction action) {OnTriggerExitEvent.AddListener(action);}
        public void RemoveEnterEvent(UnityAction action) {OnTriggerEnterEvent.RemoveListener(action);}
        public void RemoveStayEvent(UnityAction action) {OnTriggerStayEvent.RemoveListener(action);}
        public void RemoveExitEvent(UnityAction action) {OnTriggerExitEvent.RemoveListener(action);}
        public void AddEnterGOEvent(UnityAction<GameObject> action) {OnTriggerEnterGOEvent.AddListener(action);}
        public void AddStayGOEvent(UnityAction<GameObject> action) {OnTriggerStayGOEvent.AddListener(action);}
        public void AddExitGOEvent(UnityAction<GameObject> action) {OnTriggerExitGOEvent.AddListener(action);}
        public void RemoveEnterGOEvent(UnityAction<GameObject> action) {OnTriggerEnterGOEvent.RemoveListener(action);}
        public void RemoveStayGOEvent(UnityAction<GameObject> action) {OnTriggerStayGOEvent.RemoveListener(action);}
        public void RemoveExitGOEvent(UnityAction<GameObject> action) {OnTriggerExitGOEvent.RemoveListener(action);}
        public void ClearEnterEvent() {OnTriggerEnterEvent.RemoveAllListeners(); OnTriggerEnterGOEvent.RemoveAllListeners();}
        public void ClearStayEvent() {OnTriggerStayEvent.RemoveAllListeners(); OnTriggerStayGOEvent.RemoveAllListeners();}
        public void ClearExitEvent() {OnTriggerExitEvent.RemoveAllListeners(); OnTriggerExitGOEvent.RemoveAllListeners();}
#endif
    }
}