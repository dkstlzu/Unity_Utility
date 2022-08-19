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
        public DragAndDropableUI Item;
        public UnityEvent<CaseForDragAndDropUI, DragAndDropableUI> OnDropEvent;
        public UnityEvent<CaseForDragAndDropUI, DragAndDropableUI> OnDragOutEvent;
        RectTransform _rect;
        protected override void Awake()
        {
            base.Awake();
            _rect = GetComponent<RectTransform>();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (DragAndDropableUI.DragingUI != null)
            {
                if (DragAndDropableUI.DragingUI.ID != ID) return;
                
                Item = DragAndDropableUI.DragingUI;
                Item.Rect.position = _rect.position;
                Item.Case = this;
                OnDropEvent.Invoke(this, Item);
            }
        }
    }
}