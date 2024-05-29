using UnityEngine;
using UnityEngine.EventSystems;

namespace dkstlzu.Utility.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DragAndDroppableUI : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public static DragAndDroppableUI DraggingUI;
        public static DragAndDroppableUI LastDraggedUI;
        [System.NonSerialized] public RectTransform Rect;
        [System.NonSerialized] public bool SuccessfullyDropped = false;
        public int ID;
        public CaseForDragAndDropUI Case;
        public float AlphaMultiplierWhileDrag = 1;
        [SerializeField] Canvas _canvas;
        public CanvasGroup CanvasGroup{get; protected set;}

        
        [SerializeField] Vector2 _defaultPosition;
        public bool ResetIfFail;
        [SerializeField] float _defaultAlpha;
        // public float CanvasScaleFactor;

        void Awake()
        {
            Rect = GetComponent<RectTransform>();
            CanvasGroup = GetComponent<CanvasGroup>();
            _defaultAlpha = CanvasGroup.alpha;
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (Case)
            {
                Case.OnDragOutEvent?.Invoke(Case, this);
                Case.Item = null;
                Case = null;
            }
            DraggingUI = this;
            CanvasGroup.alpha = _defaultAlpha * AlphaMultiplierWhileDrag;
            CanvasGroup.blocksRaycasts = false;
            _defaultPosition = Rect.anchoredPosition;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            DraggingUI = null;
            LastDraggedUI = this;
            CanvasGroup.alpha = _defaultAlpha;
            CanvasGroup.blocksRaycasts = true;

            if (!SuccessfullyDropped && ResetIfFail)
            {
                Rect.anchoredPosition = _defaultPosition;
            }
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            // Rect.anchoredPosition += eventData.delta/CanvasScaleFactor;
            Rect.position = eventData.position;
        }
    }
}