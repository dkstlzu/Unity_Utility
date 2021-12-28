using System;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Utility.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class DropableLayoutGroup : MonoBehaviour, IDropHandler
    {
        public LayoutGroup LayoutGroup;
        RectTransform _rect;

        void Awake()
        {
            if (LayoutGroup == null)
            {
                LayoutGroup = GetComponent<LayoutGroup>();
            }
            _rect = LayoutGroup.GetComponent<RectTransform>();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (DragAndDropableUI.DragingUI != null)
            {
                if (DragAndDropableUI.DragingUI.transform.parent == LayoutGroup.transform)
                {
                    return;
                }

                DragAndDropableUI.DragingUI.transform.SetParent(LayoutGroup.transform);
                DragAndDropableUI.DragingUI.SuccessfullyDroped = true;
                OnDropCallBack();
            }
        }

        public Action OnDropCallBack;
    }
}