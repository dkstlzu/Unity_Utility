using System;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace dkstlzu.Utility.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class DroppableLayoutGroup : MonoBehaviour, IDropHandler
    {
        public LayoutGroup LayoutGroup;

        void Awake()
        {
            if (LayoutGroup == null)
            {
                LayoutGroup = GetComponent<LayoutGroup>();
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (DragAndDroppableUI.DraggingUI != null)
            {
                if (DragAndDroppableUI.DraggingUI.transform.parent == LayoutGroup.transform)
                {
                    return;
                }

                // BeforeDropCallBack(DragAndDropableUI.DragingUI, eventData);
                DragAndDroppableUI.DraggingUI.transform.SetParent(LayoutGroup.transform);
                DragAndDroppableUI.DraggingUI.SuccessfullyDropped = true;
                OnDropCallBack?.Invoke(DragAndDroppableUI.DraggingUI, eventData);
                OnLateDropCallBack?.Invoke(DragAndDroppableUI.DraggingUI, eventData);
            }
        }

        // public Action<DragAndDropableUI, PointerEventData> BeforeDropCallBack;
        public Action<DragAndDroppableUI, PointerEventData> OnDropCallBack;
        public Action<DragAndDroppableUI, PointerEventData> OnLateDropCallBack;
    }
}