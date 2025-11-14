using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace dkstlzu.Utility
{
    public class DraggableVisualElement : VisualElement
    {
        protected enum DragState
        {
            AtRest,
            PointerHover,
            Ready,
            Dragging
        }

        class DragEventHandler
        {
            public VisualElement Element;
            public EventCallback<DragUpdatedEvent> UpdateEvent;
            public EventCallback<DragPerformEvent> PerformEvent;

            public DragEventHandler(VisualElement element, EventCallback<DragUpdatedEvent> updateEvent, EventCallback<DragPerformEvent> performEvent)
            {
                Element = element;
                UpdateEvent = updateEvent;
                PerformEvent = performEvent;
            }
        }
        
        public StyleColor IdleColor = Color.black;
        public StyleColor MouseHoverColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        public StyleColor DragColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        public Dictionary<string, object> DragGenericDataDict = new Dictionary<string, object>();
        private List<DragEventHandler> _dragListenerList = new List<DragEventHandler>();
        protected DragState _dragState;
        protected string _dragAndDropDataKey = "DraggableVisualElement";

        public DraggableVisualElement()
        {
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseExit);
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseUpEvent>(OnMouseUp);

            style.backgroundColor = IdleColor;

            _dragState = DragState.AtRest;
        }
        
        private void OnMouseEnter(MouseEnterEvent evt)
        {
            style.backgroundColor = MouseHoverColor;
            if (_dragState == DragState.AtRest)
            {
                _dragState = DragState.PointerHover;
                Printer.Print($"{evt.target} MouseEnter {_dragState}", priority:-1);
            }
        }
        
        private void OnMouseExit(MouseLeaveEvent evt)
        {
            style.backgroundColor = IdleColor;
            if (_dragState == DragState.PointerHover)
            {
                _dragState = DragState.AtRest;
                Printer.Print($"{evt.target} MouseExit {_dragState}", priority:-1);
            }
        }
        
        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                _dragState = DragState.Ready;
                Printer.Print($"{evt.target} MouseDown {_dragState}", priority:-1);

                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData("PreviousMousePositionX", evt.mousePosition.x);
                DragAndDrop.SetGenericData("PreviousMousePositionY", evt.mousePosition.y);
                DragAndDrop.SetGenericData(_dragAndDropDataKey, this);
                foreach (var pair in DragGenericDataDict)
                {
                    DragAndDrop.SetGenericData(pair.Key, pair.Value);
                }

                foreach (DragEventHandler handler in _dragListenerList)
                {
                    handler.Element.RegisterCallback(handler.UpdateEvent);
                    handler.Element.RegisterCallback(handler.PerformEvent);
                }
                
                DragAndDrop.StartDrag(_dragAndDropDataKey);

                _dragState = DragState.Dragging;
                style.backgroundColor = DragColor;
                Printer.Print($"{evt.target} MouseMove {_dragState}", priority:-1);
            }
        }
        
        private void OnMouseUp(MouseUpEvent evt)
        {
            style.backgroundColor = MouseHoverColor;
            _dragState = DragState.PointerHover;
            Printer.Print($"{evt.target} MouseUp {_dragState}", priority:-1);
        }
        
        public void StopDrag()
        {
            _dragState = DragState.AtRest;
            
            foreach (DragEventHandler handler in _dragListenerList)
            {
                handler.Element.UnregisterCallback(handler.UpdateEvent);
                handler.Element.UnregisterCallback(handler.PerformEvent);
            }
            
            Printer.Print($"{this} StopDrag {_dragState}", priority:-1);
        }

        public void SetColor(Color idleColor, Color hoverColor, Color dragColor)
        { 
            IdleColor = idleColor;
            MouseHoverColor = hoverColor;
            DragColor = dragColor;

            switch (_dragState)
            {
                default:
                case DragState.AtRest:
                    style.backgroundColor = IdleColor;
                    break;
                case DragState.PointerHover:
                case DragState.Ready:
                    style.backgroundColor = MouseHoverColor;
                    break;
                case DragState.Dragging:
                    style.backgroundColor = DragColor;
                    break;
            }
        }

        public void RegisterDragCallback(VisualElement element, EventCallback<DragUpdatedEvent> dragEventHandler)
        {
            int foundIndex = _dragListenerList.FindIndex((e) => e.Element == element);

            if (foundIndex >= 0)
            {
                _dragListenerList[foundIndex].UpdateEvent += dragEventHandler;
            }
            else
            {
                _dragListenerList.Add(new DragEventHandler(element, dragEventHandler, delegate{}));                
            }
        }
        
        public void RegisterDragPerformCallback(VisualElement element, EventCallback<DragPerformEvent> dragEventHandler)
        {
            int foundIndex = _dragListenerList.FindIndex((e) => e.Element == element);

            if (foundIndex >= 0)
            {
                _dragListenerList[foundIndex].PerformEvent += dragEventHandler;
            }
            else
            {
                _dragListenerList.Add(new DragEventHandler(element, delegate{}, dragEventHandler));                
            }
        }
    }
}