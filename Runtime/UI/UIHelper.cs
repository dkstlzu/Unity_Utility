using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace dkstlzu.Utility
{
    public class UIHelper
    {
        public static TaskManagerTask ScaleOpen(RectTransform rectTransform, float time)
        {
            return new TaskManagerTask(ScaleCoroutine(rectTransform, time), true);
        }

        public static TaskManagerTask ScaleClose(RectTransform rectTransform, float time)
        {
            return new TaskManagerTask(ScaleCoroutine(rectTransform, -time), true);
        }

        static IEnumerator ScaleCoroutine(RectTransform rectTransform, float time)
        {
            Vector3 initialScale = rectTransform.localScale;
            float delta = 0;

            if (time > 0)
            {
                while(rectTransform.localScale.x < 1)
                {
                    delta = Time.deltaTime/time;
                    rectTransform.localScale += new Vector3(delta, delta, 0);
                    yield return null;
                }
            } else if (time < 0)
            {
                while(rectTransform.localScale.x > 0)
                {
                    delta = Time.deltaTime/time;
                    rectTransform.localScale += new Vector3(delta, delta, 0);
                    yield return null;
                }
            }
        }
    }
}