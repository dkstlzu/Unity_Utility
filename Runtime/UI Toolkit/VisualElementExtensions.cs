using UnityEngine;
using UnityEngine.UIElements;

namespace dkstlzu.Utility
{
    public static class VisualElementExtensions
    {
        /// <summary>
        /// TopLeft를 원점으로 기반합니다.
        /// Position을 Absolute로 변경하니 유의합시다.
        /// </summary>
        public static void SetPosition(this VisualElement element, Vector2 position)
        {
            element.style.position = Position.Absolute;
            element.style.left = new StyleLength(position.x);
            element.style.top = new StyleLength(position.y);
        }

        /// <summary>
        /// TopLeft를 원점으로 기반합니다.
        /// Position을 Absolute로 변경하니 유의합시다.
        /// </summary>
        public static void SetPositionWithPercent(this VisualElement element, Vector2 position)
        {
            element.style.position = Position.Absolute;
            element.style.left = new StyleLength(Length.Percent(position.x));
            element.style.top = new StyleLength(Length.Percent(position.y));
        }

        /// <summary>
        /// TopLeft를 원점으로 기반합니다.
        /// Position을 Absolute로 변경하니 유의합시다.
        /// </summary>
        public static void SetSize(this VisualElement element, Vector2 size)
        {
            element.style.width = new StyleLength(size.x);
            element.style.height = new StyleLength(size.y);
        }
        
        /// <summary>
        /// TopLeft를 원점으로 기반합니다.
        /// Position을 Absolute로 변경하니 유의합시다.
        /// </summary>
        public static void SetSizeWithPercent(this VisualElement element, Vector2 size)
        {
            element.style.width = new StyleLength(Length.Percent(size.x));
            element.style.height = new StyleLength(Length.Percent(size.y));
        }

        /// <summary>
        /// TopLeft를 원점으로 기반합니다.
        /// Position을 Absolute로 변경하니 유의합시다.
        /// </summary>
        public static void SetPositionAndSize(this VisualElement element, Vector2 position, Vector2 size)
        {
            element.SetPosition(position);
            element.SetSize(size);
        }
        
        /// <summary>
        /// TopLeft를 원점으로 기반합니다.
        /// Position을 Absolute로 변경하니 유의합시다.
        /// position을 center로 설정합니다.
        /// </summary>
        public static void SetCenterAndSize(this VisualElement element, Vector2 center, Vector2 size)
        {
            Vector2 position = new Vector2(center.x - size.x, center.y - size.y);
            element.SetPositionAndSize(position, size);
        }

        /// <summary>
        /// TopLeft를 원점으로 기반합니다.
        /// Position을 Absolute로 변경하니 유의합시다.
        /// </summary>
        public static void SetPositionAndSizeWithPercent(this VisualElement element, Vector2 position, Vector2 size)
        {
            element.SetPositionWithPercent(position);
            element.SetSizeWithPercent(size);
        }
        
        /// <summary>
        /// TopLeft를 원점으로 기반합니다.
        /// Position을 Absolute로 변경하니 유의합시다.
        /// </summary>
        public static void SetCenterAndSizeWithRatio(this VisualElement element, Vector2 center, Vector2 size)
        {
            Vector2 position = new Vector2(center.x - size.x/element.layout.width, center.y - size.y/element.layout.height);
            element.SetPositionAndSizeWithPercent(position, size);
        }
        
        public static void SwitchDisplay(this VisualElement element)
        {
            element.style.display = element.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public static void SwitchDisplay(this VisualElement element, bool on)
        {
            element.style.display = on ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        public static void SetImageFitableVisualElement(this Image image)
        {
            if (image.sprite == null) return;

            VisualElement parent = image.parent;
            float layoutRatio = parent.layout.height/parent.layout.width;
            float targetRatio = image.sprite.rect.height / image.sprite.rect.width;
            
            if (layoutRatio >= targetRatio)
            {
                float targetRectHeight = image.layout.width * targetRatio;
                float topBottomPosition = (parent.layout.height - targetRectHeight) / 2;

                image.style.left = 0;
                image.style.right = 0;
                image.style.top = topBottomPosition;
                image.style.bottom = topBottomPosition;
            }
            else
            {
                float targetRectWidth = image.layout.height / targetRatio;
                float leftRightPadding = (parent.layout.width - targetRectWidth) / 2;
                
                image.style.left = leftRightPadding;
                image.style.right = leftRightPadding;
                image.style.top = 0;
                image.style.bottom = 0;
            }
        }
    }
}