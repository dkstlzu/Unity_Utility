using System.Collections.Generic;
using UnityEngine;

namespace dkstlzu.Utility.UI
{
    public class RatioLayoutGroup : MonoBehaviour
    {
        [Header("Row, Column")]
        public bool RowFirst;
        public int Row;
        public int Column;

        [Header("Padding")]
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;


        public List<RectTransform> Children = new List<RectTransform>();

        [ContextMenu("FindChildren")]
        public void FindChildren()
        {
            Children.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                Children.Add(transform.GetChild(i).GetComponent<RectTransform>());
            }
        }

        [ContextMenu("Arrange")]
        public void Arrange()
        {
            float rowRatio = 1f/Row;
            float columnRatio = 1f/Column;

            if (!RowFirst)
            {
                for (int i = 0; i < Row; i++)
                {
                    for (int j = 0; j < Column; j++)
                    {
                        Children[(i*Column)+j].anchorMin = new Vector2(columnRatio * j, 1 - rowRatio * (i+1));
                        Children[(i*Column)+j].anchorMax = new Vector2(columnRatio * (j+1), 1 - rowRatio * i);
                        Children[(i*Column)+j].sizeDelta = new Vector2(Left, Top);
                    }
                }
            } else
            {
                for (int i = 0; i < Column; i++)
                {
                    for (int j = 0; j < Row; j++)
                    {
                        Children[(i*Column)+j].anchorMin = new Vector2(columnRatio * j, 1 - rowRatio * (i+1));
                        Children[(i*Column)+j].anchorMax = new Vector2(columnRatio * (j+1), 1 - rowRatio * i);
                        Children[(i*Column)+j].sizeDelta = new Vector2(Left, Top);
                    }
                }
            }
        }
    }
}
