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

        void Awake()
        {
            if (LayoutGroup == null)
            {
                LayoutGroup = GetComponent<LayoutGroup>();
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (DragAndDropableUI.DragingUI != null)
            {
                if (DragAndDropableUI.DragingUI.transform.parent == LayoutGroup.transform)
                {
                    return;
                }

                // BeforeDropCallBack(DragAndDropableUI.DragingUI, eventData);
                DragAndDropableUI.DragingUI.transform.SetParent(LayoutGroup.transform);
                DragAndDropableUI.DragingUI.SuccessfullyDroped = true;
                OnDropCallBack(DragAndDropableUI.DragingUI, eventData);
                AfterDropCallBack(DragAndDropableUI.DragingUI, eventData);
            }
        }

        // public Action<DragAndDropableUI, PointerEventData> BeforeDropCallBack;
        public Action<DragAndDropableUI, PointerEventData> OnDropCallBack;
        public Action<DragAndDropableUI, PointerEventData> AfterDropCallBack;
    }
}