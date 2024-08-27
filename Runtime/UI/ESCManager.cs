using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Comparer for comparing two keys, handling equality as beeing greater
        /// Use this Comparer e.g. with SortedLists or SortedDictionaries, that don't allow duplicate keys
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        public class DuplicateKeyComparer<TKey>
            :
                IComparer<TKey> where TKey : IComparable
        {
            #region IComparer<TKey> Members

            public int Compare(TKey x, TKey y)
            {
                int result = x.CompareTo(y);

                if (result == 0)
                    return 1; // Handle equality as being greater. Note: this will break Remove(key) or
                else          // IndexOfKey(key) since the comparer never returns 0 to signal key equality
                    return result;
            }

            #endregion
        }
        
        private SortedList<int, Action> _pq = new SortedList<int, Action>(new DuplicateKeyComparer<int>());

        public void ESC()
        {
            var keys = _pq.Keys;

            for (int i = 0; i < keys.Count; i++)
            {
                try
                {
                    _pq[keys[i]]?.Invoke();
                    _pq.Remove(keys[i]);
                    return;
                }
                catch (Exception )
                {
                    _pq.Remove(keys[i]);
                }
            }
        }

        public void AddItem(Action action, int priority)
        {
            _pq.TryAdd(priority, action);
        }
    }
}