using UnityEngine;

namespace dkstlzu.Utility
{
    public class UIOpenClose : MonoBehaviour
    {
        public RectTransform TargetUI;
        public string ESCManagerItemKey => TargetUI.GetHashCode() + "UIOpenClose";

        void Reset()
        {
            TargetUI = GetComponent<RectTransform>();
        }

        /// <summary>
        /// On Button Reference
        /// </summary>
        public void Open()
        {
            if (UIHelper.ScaleOpen(TargetUI))
                ESCManager.instance.AddItem(TargetUI.GetHashCode() + "UIOpenClose", () => Close(), 0);
        }

        /// <summary>
        /// On Button Reference
        /// </summary>
        public void Close()
        {
            if (UIHelper.ScaleClose(TargetUI))
                ESCManager.instance.RemoveItem(TargetUI.GetHashCode() + "UIOpenClose");
            else
                ESCManager.instance.AddItem(TargetUI.GetHashCode() + "UIOpenClose", () => Close(), 0);
        }
    }
}