using UnityEngine;
using UnityEngine.EventSystems;

namespace Utility.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DragAndDropableUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public CanvasGroup CanvasGroup;
        public RectTransform Rect;
        public bool SuccessfullyDroped = false;

        [SerializeField] Vector2 _defaultPosition;

        void Awake()
        {
            Rect = GetComponent<RectTransform>();
            CanvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {

        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {

        }
        
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            DragAndDropUI.instance.DragingUI = this;
            CanvasGroup.alpha = 0.6f;
            CanvasGroup.blocksRaycasts = false;
            _defaultPosition = Rect.anchoredPosition;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            print("OnDragEnd");
            DragAndDropUI.instance.DragingUI = null;
            DragAndDropUI.instance.LastDragUI = this;
            CanvasGroup.alpha = 1f;
            CanvasGroup.blocksRaycasts = true;

            if (!SuccessfullyDroped)
            {
                print("Reset Position");
                Rect.anchoredPosition = _defaultPosition;
            }
            SuccessfullyDroped = false;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            Rect.anchoredPosition += eventData.delta / DragAndDropUI.instance.TargetCanvas.scaleFactor;
        }
    }
}