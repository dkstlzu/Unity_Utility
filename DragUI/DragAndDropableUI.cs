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
        public float CanvasScaleFactor;
        public static Canvas DraggingCanvas;
        public GameObject RenderingUIElements;
        CanvasGroup CanvasGroup;

        GameObject DraggingGameObject;
        RectTransform DraggingGameObjectRect;

        [SerializeField] Vector2 _defaultPosition;

        void Awake()
        {
            Rect = GetComponent<RectTransform>();
            CanvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (DraggingCanvas == null)
            {
                DraggingCanvas = new GameObject("Dragging Canvas").AddComponent<Canvas>();
                DraggingCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            }


            DraggingGameObject = Instantiate(gameObject, transform.position, transform.rotation);
            if (RenderingUIElements) RenderingUIElements.SetActive(false);
            Destroy(DraggingGameObject.GetComponent<DragAndDropableUI>());
            Destroy(DraggingGameObject.GetComponent<CanvasGroup>());
            DraggingGameObject.transform.SetParent(DraggingCanvas.transform);
            DraggingGameObjectRect = DraggingGameObject.GetComponent<RectTransform>();
            DraggingGameObjectRect.sizeDelta = new Vector2(Rect.rect.width, Rect.rect.height);
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (RenderingUIElements) RenderingUIElements.SetActive(true);
            Destroy(DraggingGameObject);
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
            Rect.anchoredPosition += eventData.delta/CanvasScaleFactor;
            DraggingGameObjectRect.anchoredPosition += eventData.delta/CanvasScaleFactor;
        }
    }
}