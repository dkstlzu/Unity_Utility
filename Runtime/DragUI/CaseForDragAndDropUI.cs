using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace dkstlzu.Utility.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class CaseForDragAndDropUI : Graphic, IDropHandler
    {
        public int ID;
        public bool CannotTakeAgain;
        public DragAndDroppableUI Item;
        public UnityEvent<CaseForDragAndDropUI, DragAndDroppableUI> OnDropEvent;
        public UnityEvent<CaseForDragAndDropUI, DragAndDroppableUI> OnDragOutEvent;
        RectTransform _rect;
        protected override void Awake()
        {
            base.Awake();
            _rect = GetComponent<RectTransform>();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (DragAndDroppableUI.DraggingUI != null)
            {
                if (DragAndDroppableUI.DraggingUI.ID != ID) return;
                
                Item = DragAndDroppableUI.DraggingUI;
                Item.Rect.position = _rect.position;
                Item.Case = this;
                Item.CanvasGroup.blocksRaycasts = false;
                OnDropEvent.Invoke(this, Item);
            }
        }
    }
}