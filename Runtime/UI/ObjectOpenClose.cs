using System;
using System.Collections.Generic;
using UnityEngine;

namespace dkstlzu.Utility
{
    public class ObjectOpenClose : MonoBehaviour
    {
        public Transform Target;
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
            if (UIHelper.ScaleOpen(Target))
            {
                ESCManager.instance.AddItem(ESCManagerItemKey, Close, 0);
                OnOpen?.Invoke();
                CoroutineHelper.Delay(AfterOpen, 1.1f);
            }
        }

        /// <summary>
        /// On Button Reference
        /// </summary>
        [ContextMenu("Close")]
        public void Close()
        {
            if (UIHelper.ScaleClose(Target))
            {
                ESCManager.instance.RemoveItem(ESCManagerItemKey);
                OnClose?.Invoke();
                CoroutineHelper.Delay(AfterClose, 1.1f);
            } else
            {
                ESCManager.instance.AddItem(ESCManagerItemKey, Close, 0);
            }
        }
    }
}