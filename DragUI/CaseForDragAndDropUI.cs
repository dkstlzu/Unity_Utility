using UnityEngine;
using UnityEngine.EventSystems;

namespace Utility.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class CasaForDragAndDropUI : MonoBehaviour, IDropHandler
    {
        RectTransform _rect;
        void Awake()
        {
            _rect = GetComponent<RectTransform>();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (DragAndDropUI.instance.DragingUI != null)
            {
                DragAndDropUI.instance.DragingUI.Rect.anchoredPosition = _rect.anchoredPosition;
            }
        }
    }
}