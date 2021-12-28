using UnityEngine;
using UnityEngine.EventSystems;

namespace Utility.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DragAndDropableUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public static DragAndDropableUI DragingUI;
        public static DragAndDropableUI LastDraggedUI;
        [System.NonSerialized] public RectTransform Rect;
        [System.NonSerialized] public bool SuccessfullyDroped = false;
        CanvasGroup CanvasGroup;

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
            DragingUI = this;
            CanvasGroup.alpha = 0.6f;
            CanvasGroup.blocksRaycasts = false;
            _defaultPosition = Rect.anchoredPosition;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            DragingUI = null;
            LastDraggedUI = this;
            CanvasGroup.alpha = 1f;
            CanvasGroup.blocksRaycasts = true;

            if (!SuccessfullyDroped)
            {
                Rect.anchoredPosition = _defaultPosition;
            }
            SuccessfullyDroped = false;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            Rect.anchoredPosition += eventData.delta/GetComponent<Canvas>().scaleFactor;
        }
    }
}