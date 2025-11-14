using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace dkstlzu.Utility
{
    public class RectElement : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<RectElement, UxmlTraits> {}

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlFloatAttributeDescription _x = new UxmlFloatAttributeDescription() { name = "x", defaultValue = 0 };
            private UxmlFloatAttributeDescription _y = new UxmlFloatAttributeDescription() { name = "y", defaultValue = 0 };
            private UxmlFloatAttributeDescription _width = new UxmlFloatAttributeDescription() { name = "width", defaultValue = 0 };
            private UxmlFloatAttributeDescription _height = new UxmlFloatAttributeDescription() { name = "height", defaultValue = 0 };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription { get { yield break; } }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                RectElement re = ve as RectElement;

                re.x = _x.GetValueFromBag(bag, cc);
                re.y = _y.GetValueFromBag(bag, cc);
                re.width = _width.GetValueFromBag(bag, cc);
                re.height = _height.GetValueFromBag(bag, cc);
            }
        }
        
        private Rect m_Rect;

        public float x
        {
            get => m_Rect.x;
            set
            {
                m_Rect.x = value;
                style.left = value;
            }
        }

        public float y
        {
            get => m_Rect.y;
            set
            {
                m_Rect.y = value;
                style.bottom = value;
            }
        }

        public float width
        {
            get => m_Rect.width;
            set
            {
                float widthDelta = value - m_Rect.width;
                
                m_Rect.width = value;
                style.width = value;

                _topLine.SetPoint2(_topLine.Point2 + Vector2.right * widthDelta);
                _bottomLine.SetPoint2(_bottomLine.Point2 + Vector2.right * widthDelta);
                _rightLine.SetPoints(_rightLine.Point1 + Vector2.right * widthDelta, _rightLine.Point2 + Vector2.right * widthDelta);
            }
        }

        public float height
        {
            get => m_Rect.height;
            set
            {
                float heightDelta = value - m_Rect.height;

                m_Rect.height = value;
                style.height = m_Rect.height;
                
                _leftLine.SetPoint2(_leftLine.Point2 + Vector2.up * heightDelta);
                _rightLine.SetPoint2(_rightLine.Point2 + Vector2.up * heightDelta);
                _topLine.SetPoints(_topLine.Point1 + Vector2.up * heightDelta, _topLine.Point2 + Vector2.up * heightDelta);
            }
        }

        public Vector2 center
        {
            get => m_Rect.center;
            set
            {
                m_Rect.center = value;
                x = center.x - width / 2;
                y = center.y - height / 2;
            }
        }

        public Vector2 size
        {
            get => m_Rect.size;
            set
            {
                width = value.x;
                height = value.y;
            }
        }

        public Rect Rect
        {
            get => m_Rect;
            set
            {
                size = value.size;
                center = value.center;
            }
        }

        public const string DRAGANDDROP_DATA_KEY = "RectElement";

        private DotElement _topLeftVertex;
        private DotElement _topRightVertex;
        private DotElement _bottomLeftVertex;
        private DotElement _bottomRightVertex;

        private LineElement _leftLine;
        private LineElement _rightLine;
        private LineElement _topLine;
        private LineElement _bottomLine;

        public RectElement() : this(new Rect())
        {
        }

        public RectElement(Rect rect)
        {
            m_Rect = new Rect();
            style.position = Position.Absolute;
            AddLines();
            AddDots();

            Rect = rect;
        }

        void AddDots()
        {
            _topLeftVertex = new DotElement();
            _topRightVertex = new DotElement();
            _bottomLeftVertex = new DotElement();
            _bottomRightVertex = new DotElement();

            _topLeftVertex.style.position = Position.Absolute;
            _topRightVertex.style.position = Position.Absolute;
            _bottomLeftVertex.style.position = Position.Absolute;
            _bottomRightVertex.style.position = Position.Absolute;

            _topLeftVertex.style.top = 0;
            _topLeftVertex.style.left = 0;
            _topRightVertex.style.top = 0;
            _topRightVertex.style.right = 0;
            _bottomLeftVertex.style.bottom = 0;
            _bottomLeftVertex.style.left = 0;
            _bottomRightVertex.style.bottom = 0;
            _bottomRightVertex.style.right = 0;
            
            _topLeftVertex.DragGenericDataDict.Add(DRAGANDDROP_DATA_KEY, this);
            _topRightVertex.DragGenericDataDict.Add(DRAGANDDROP_DATA_KEY, this);
            _bottomLeftVertex.DragGenericDataDict.Add(DRAGANDDROP_DATA_KEY, this);
            _bottomRightVertex.DragGenericDataDict.Add(DRAGANDDROP_DATA_KEY, this);
            
            Add(_topLeftVertex);
            Add(_topRightVertex);
            Add(_bottomLeftVertex);
            Add(_bottomRightVertex);
        }

        void AddLines()
        {
            _leftLine = new LineElement();
            _rightLine = new LineElement();
            _topLine = new LineElement();
            _bottomLine = new LineElement();

            _leftLine.LineWidth = 3;
            _rightLine.LineWidth = 3;
            _topLine.LineWidth = 3;
            _bottomLine.LineWidth = 3;
            
            _leftLine.DragGenericDataDict.Add(DRAGANDDROP_DATA_KEY, this);
            _rightLine.DragGenericDataDict.Add(DRAGANDDROP_DATA_KEY, this);
            _topLine.DragGenericDataDict.Add(DRAGANDDROP_DATA_KEY, this);
            _bottomLine.DragGenericDataDict.Add(DRAGANDDROP_DATA_KEY, this);
            
            Add(_leftLine);
            Add(_rightLine);
            Add(_topLine);
            Add(_bottomLine);
        }

        public void SetDraggableAreaVisualElement(VisualElement element)
        {
            _topLeftVertex.RegisterDragCallback(element, OnDrag);
            _topRightVertex.RegisterDragCallback(element, OnDrag);
            _bottomLeftVertex.RegisterDragCallback(element, OnDrag);
            _bottomRightVertex.RegisterDragCallback(element, OnDrag);
            
            _leftLine.RegisterDragCallback(element, OnDrag);
            _rightLine.RegisterDragCallback(element, OnDrag);
            _topLine.RegisterDragCallback(element, OnDrag);
            _bottomLine.RegisterDragCallback(element, OnDrag);
            
            _topLeftVertex.RegisterDragPerformCallback(element, OnDragEnd);
            _topRightVertex.RegisterDragPerformCallback(element, OnDragEnd);
            _bottomLeftVertex.RegisterDragPerformCallback(element, OnDragEnd);
            _bottomRightVertex.RegisterDragPerformCallback(element, OnDragEnd);
            
            _leftLine.RegisterDragPerformCallback(element, OnDragEnd);
            _rightLine.RegisterDragPerformCallback(element, OnDragEnd);
            _topLine.RegisterDragPerformCallback(element, OnDragEnd);
            _bottomLine.RegisterDragPerformCallback(element, OnDragEnd);
        }

        private void OnDrag(DragUpdatedEvent evt)
        {
            Rect previousValue = Rect;
            
            DotElement draggingDot = DragAndDrop.GetGenericData(DotElement.DRAGANDDROP_DATA_KEY) as DotElement;
            LineElement draggingLine = DragAndDrop.GetGenericData(LineElement.DRAGANDDROP_DATA_KEY) as LineElement;
            if (draggingDot == null && draggingLine == null) return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            
            float previousMousePositionX = (float)DragAndDrop.GetGenericData("PreviousMousePositionX");
            float previousMousePositionY = (float)DragAndDrop.GetGenericData("PreviousMousePositionY");

            float xDelta = evt.mousePosition.x - previousMousePositionX;
            // VisualElement와 Rect의 y기준이 반대입니다
            float yDelta = -(evt.mousePosition.y - previousMousePositionY);
            
            if (draggingDot == _topLeftVertex)
            {
                x += xDelta;
                width -= xDelta;
                height += yDelta;
            } else if (draggingDot == _topRightVertex)
            {
                width += xDelta;
                height += yDelta;
            } else if (draggingDot == _bottomLeftVertex)
            {
                x += xDelta;
                width -= xDelta;
                y += yDelta;
                height -= yDelta;
            } else if (draggingDot == _bottomRightVertex)
            {
                width += xDelta;
                y += yDelta;
                height -= yDelta;
            } else if (draggingLine == _leftLine)
            {
                x += xDelta;
                width -= xDelta;
            } else if (draggingLine == _rightLine)
            {
                width += xDelta;
            } else if (draggingLine == _topLine)
            {
                height += yDelta;
            } else if (draggingLine == _bottomLine)
            {
                y += yDelta;
                height -= yDelta;
            }
            else
            {
                return;
            }
            
            DragAndDrop.SetGenericData("PreviousMousePositionX", evt.mousePosition.x);
            DragAndDrop.SetGenericData("PreviousMousePositionY", evt.mousePosition.y);
                        
            // 🔔 이벤트 발행
            ChangeEvent<Rect> valueChangeEvent = ChangeEvent<Rect>.GetPooled(previousValue, Rect);
            valueChangeEvent.target = this;
            SendEvent(valueChangeEvent);
        }
        
        private void OnDragEnd(DragPerformEvent evt)
        {
            DotElement draggedDot = DragAndDrop.GetGenericData(DotElement.DRAGANDDROP_DATA_KEY) as DotElement;
            LineElement draggedLine = DragAndDrop.GetGenericData(LineElement.DRAGANDDROP_DATA_KEY) as LineElement;

            DragAndDrop.AcceptDrag();
            
            if (draggedDot != null)
            {
                draggedDot.StopDrag();
            }

            if (draggedLine != null)
            {
                draggedLine.StopDrag();
            }
        }

        public void SetColor(Color idleColor, Color hoverColor, Color dragColor)
        {
            _topLeftVertex.SetColor(idleColor, hoverColor, dragColor);
            _topRightVertex.SetColor(idleColor, hoverColor, dragColor);
            _bottomLeftVertex.SetColor(idleColor, hoverColor, dragColor);
            _bottomRightVertex.SetColor(idleColor, hoverColor, dragColor);
            
            _leftLine.SetColor(idleColor, hoverColor, dragColor);
            _rightLine.SetColor(idleColor, hoverColor, dragColor);
            _topLine.SetColor(idleColor, hoverColor, dragColor);
            _bottomLine.SetColor(idleColor, hoverColor, dragColor);
        }
    }
}