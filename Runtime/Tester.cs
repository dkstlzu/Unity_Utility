using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace dkstlzu.Utility
{
    public class Tester : MonoBehaviour
    {
        public Vector2 TestButtonStartPosition = new Vector2(100, 100);
        public float TestButtonInterval = 20;
        public int FontSize = 20;
        public bool VerticalArrange;
        public List<TestButton> TestButtonList;

        protected void OnGUI()
        {
            float x = TestButtonStartPosition.x;
            float y = TestButtonStartPosition.y;
            GUIStyle tempGUIStyle = new GUIStyle(GUI.skin.button);
            tempGUIStyle.fontSize = FontSize;

            for (int i = 0; i < TestButtonList.Count; i++)
            {
                TestButton temp = TestButtonList[i];
                if (!VerticalArrange)
                {
                    if (GUI.Button(rect(x, y, temp.TestButtonRectSize), temp.Text, tempGUIStyle))
                    {
                        temp.TestAction?.Invoke();
                    }
                    x += temp.TestButtonRectSize.x + TestButtonInterval;
                } else
                {
                    if (GUI.Button(rect(x, y, temp.TestButtonRectSize), temp.Text, tempGUIStyle))
                    {
                        temp.TestAction?.Invoke();
                    }
                    y += temp.TestButtonRectSize.y + TestButtonInterval;
                }

            }
        }

        Rect rect(float x, float y, Vector2 size)
        {
            return new Rect(x, y, size.x, size.y);
        }

        [ContextMenu("AddTestButton")]
        public void AddTestButton()
        {
            TestButtonList.Add(new TestButton());
        }
    }

    [Serializable]
    public class TestButton
    {
        public string Text = "Do Test";
        public Vector2 TestButtonRectSize = new Vector2(100, 50);
        public Action TestAction;

        public TestButton() {}
        public TestButton(Action action)
        {
            TestAction = action;
        }

        public TestButton(Action action, string text)
        {
            TestAction = action;
            Text = text;
        }
    }
}