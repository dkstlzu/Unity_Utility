using UnityEngine;
using UnityEngine.UIElements;

namespace dkstlzu.Utility
{
    public class DotElement : DraggableVisualElement
    {
        public new class UxmlFactory : UxmlFactory<DotElement, UxmlTraits>{}

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlFloatAttributeDescription _radius = new UxmlFloatAttributeDescription(){name = "Radius", defaultValue = 0};

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                DotElement dot = ve as DotElement;

                dot.Radius = _radius.GetValueFromBag(bag, cc);
            }
        }
        
        public const int DEFAULT_RADIUS = 10;
        public const string DRAGANDDROP_DATA_KEY = "DotElement";

        public float Radius { get; set; }

        public DotElement() : this(DEFAULT_RADIUS)
        {
            
        }

        public DotElement(float radius) : base()
        {
            _dragAndDropDataKey = DRAGANDDROP_DATA_KEY;

            Radius = radius;
            
            style.borderTopLeftRadius = Radius;
            style.borderTopRightRadius = Radius;
            style.borderBottomLeftRadius = Radius;
            style.borderBottomRightRadius = Radius;
            style.width = Radius;
            style.height = Radius;
        }
    }
}