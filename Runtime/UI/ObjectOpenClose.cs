using System;
using System.Collections.Generic;
using UnityEngine;

namespace dkstlzu.Utility
{
    public class ObjectOpenClose : MonoBehaviour
    {
        public bool isOpened;
        public Transform Target;
        public float Duration = 1;
        public string ESCManagerItemKey => Target.GetHashCode() + "ObjectOpenClose";
        public event Action OnOpen = delegate{};
        public event Action OnClose = delegate{};
        public event Action AfterOpen = delegate{};
        public event Action AfterClose = delegate{};

        void Reset()
        {
            Target = GetComponent<Transform>();
        }

        /// <summary>
        /// On Button Reference
        /// </summary>
        [ContextMenu("Open")]
        public void Open()
        {
            if (UIHelper.ScaleOpen(Target, Duration))
            {
                ESCManager.instance.AddItem(ESCManagerItemKey, Close, 0);
                OnOpen?.Invoke();
                CoroutineHelper.Delay(AfterOpen, Duration + 0.1f);
                isOpened = true;
            }
        }

        /// <summary>
        /// On Button Reference
        /// </summary>
        [ContextMenu("Close")]
        public void Close()
        {
            if (UIHelper.ScaleClose(Target, Duration))
            {
                ESCManager.instance.RemoveItem(ESCManagerItemKey);
                OnClose?.Invoke();
                CoroutineHelper.Delay(AfterClose, Duration + 0.1f);
                isOpened = false;
            } else
            {
                ESCManager.instance.AddItem(ESCManagerItemKey, Close, 0);
            }
        }
    }
}