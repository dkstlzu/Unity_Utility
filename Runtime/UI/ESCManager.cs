using System;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using Context = UnityEngine.InputSystem.InputAction.CallbackContext;
#endif


namespace dkstlzu.Utility.UI
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

        class ActionListBasedPriorityQueue : ListBasedPriorityQueue<Action> 
        {
            public override void AddItem(string name, Item item)
            {
                if (!ItemDict.ContainsKey(name))
                {
                    ItemDict.Add(name, item);
                } else
                {
                    ItemDict[name].Element += item.Element;
                }
            }
        }

        private ActionListBasedPriorityQueue PQ = new ActionListBasedPriorityQueue();


        public void ESC()
        {
            ListBasedPriorityQueue<Action>.Item item = PQ.Peek();
            if (item != null) item.Element();
        }

        public void AddItem(string name, Action action, int priority)
        {
            PQ.AddItem(name, action, priority);
        }

        public ListBasedPriorityQueue<Action>.Item RemoveItem(string name)
        {
            return PQ.RemoveItem(name);
        }
    }
}