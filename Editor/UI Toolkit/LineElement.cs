using UnityEngine;
using UnityEngine.UIElements;

namespace dkstlzu.Utility
{
    public class LineElement : DraggableVisualElement
    {
        public new class UxmlFactory : UxmlFactory<LineElement, UxmlTraits> {}
        
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlFloatAttributeDescription _lineWidth = new UxmlFloatAttributeDescription() { name = "LineWidth", defaultValue = 1 };
            private UxmlFloatAttributeDescription _point1X = new UxmlFloatAttributeDescription() { name = "Point1X", defaultValue = 0 };
            private UxmlFloatAttributeDescription _point1Y = new UxmlFloatAttributeDescription() { name = "Point1Y", defaultValue = 0 };
            private UxmlFloatAttributeDescription _point2X = new UxmlFloatAttributeDescription() { name = "Point2X", defaultValue = 0 };
            private UxmlFloatAttributeDescription _point2Y = new UxmlFloatAttributeDescription() { name = "Point2Y", defaultValue = 0 };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                LineElement line = ve as LineElement;

                line.LineWidth = _lineWidth.GetValueFromBag(bag, cc);
                line.SetPoints(new Vector2(_point1X.GetValueFromBag(bag, cc), _point1Y.GetValueFromBag(bag, cc)), 
                    new Vector2(_point2X.GetValueFromBag(bag, cc), _point2Y.GetValueFromBag(bag, cc)));
            }
        }

        public const int DEFAULT_WIDTH = 1;
        public const string DRAGANDDROP_DATA_KEY = "LineElement";

        public Vector2 Point1 { get; private set; }
        public Vector2 Point2 { get; private set; }
        public float AngleInRadian { get; private set; }
        private float _lineWidth;
        public float LineWidth
        {
            get => _lineWidth;
            set
            {
                _lineWidth = value;
                style.height = value;
                style.bottom = Point1.y - value / 2;
            }
        }
        
        public LineElement() : this(DEFAULT_WIDTH)
        {
            
        }
        
        public LineElement(float width) : this(width, Vector2.zero, Vector2.zero)
        {
            
        }
        
        public LineElement(float width, Vector2 point1, Vector2 point2)
        {
            _dragAndDropDataKey = DRAGANDDROP_DATA_KEY;

            LineWidth = width;
            Point1 = point1;
            Point2 = point2;
            
            style.position = Position.Absolute;
            style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(Length.Percent(0), Length.Percent(50)));

            RegenerateLine();
        }

        public void SetPoints(Vector2 point1, Vector2 point2)
        {
            Point1 = point1;
            Point2 = point2;
            RegenerateLine();
        }

        public void SetPoint1(Vector2 point)
        {
            Point1 = point;
            RegenerateLine();
        }

        public void SetPoint2(Vector2 point)
        {
            Point2 = point;
            RegenerateLine();
        }

        private void RegenerateLine()
        {
            Vector2 diff = Point2 - Point1;
            style.width = diff.magnitude;

            style.left = Point1.x;
            style.bottom = Point1.y;

            // VisualElement의 y축이 반전되어있습니다
            if (diff.x >= 0)
            {
                AngleInRadian = -Mathf.Atan(diff.y / diff.x);
            }
            else
            {
                AngleInRadian = -(Mathf.Atan(diff.y / diff.x) + Mathf.PI);
            }
            style.rotate = new StyleRotate(new Rotate(new Angle(AngleInRadian, AngleUnit.Radian)));
        }
    }
}