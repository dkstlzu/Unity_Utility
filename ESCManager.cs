using System;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using Context = UnityEngine.InputSystem.InputAction.CallbackContext;
#endif


namespace Utility
{
    public class ESCManager : Singleton<ESCManager>
    {

#if ENABLE_INPUT_SYSTEM
        public InputAction ESCInputAction = new InputAction("ESCInputAction", InputActionType.Button, "<Keyboard>/escape");

        void Awake()
        {
            ESCInputAction.performed += ESC;
        }

        void ESC(Context context)
        {
            ESC();
        }

        void OnEnable()
        {
            ESCInputAction.Enable();
        }

        void Disable()
        {
            ESCInputAction.Disable();
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ESC();
            }
        }
#endif

        class ActionPriorityQueue : PriorityQueue<Action> {}
        private ActionPriorityQueue PQ = new ActionPriorityQueue();


        public void ESC()
        {
            PriorityQueue<Action>.Item item = PQ.Pop();
            if (item != null) item.Element();
        }

        public void AddItem(string name, Action action, int priority)
        {
            PQ.AddItem(name, action, priority);
        }

        public PriorityQueue<Action>.Item RemoveItem(string name)
        {
            return PQ.RemoveItem(name);
        }
    }
}