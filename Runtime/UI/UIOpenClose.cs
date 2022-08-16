using System;
using System.Collections.Generic;
using UnityEngine;

namespace dkstlzu.Utility
{
    public class UIOpenClose : MonoBehaviour
    {
        public RectTransform TargetUI;
        public string ESCManagerItemKey => TargetUI.GetHashCode() + "UIOpenClose";

        List<Action> _closeActionList = new List<Action>();

        void Reset()
        {
            TargetUI = GetComponent<RectTransform>();
        }

        /// <summary>
        /// On Button Reference
        /// </summary>
        public void Open()
        {
            Open(null);
        }

        public void Open(IEnumerable<Action> closeActions)
        {
            if (UIHelper.ScaleOpen(TargetUI))
                ESCManager.instance.AddItem(ESCManagerItemKey, () => Close(), 0);
            
            if (closeActions != null)
                _closeActionList.AddRange(closeActions);
        }

        /// <summary>
        /// On Button Reference
        /// </summary>
        public void Close()
        {
            if (UIHelper.ScaleClose(TargetUI))
            {
                ESCManager.instance.RemoveItem(ESCManagerItemKey);
                foreach(Action action in _closeActionList)
                {
                    action?.Invoke();
                }
            }
        }
    }
}