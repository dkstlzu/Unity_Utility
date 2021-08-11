using UnityEngine;
using UnityEngine.EventSystems;

namespace Utility.UI
{
    public class DragAndDropUI : Singleton<DragAndDropUI>
    {
        public Canvas TargetCanvas;
        public DragAndDropableUI DragingUI;
        public DragAndDropableUI LastDragUI;
    }
}