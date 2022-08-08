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
            return new TaskManagerTask(ScaleTimeOpenCoroutine(rectTransform, time), true);
        }

        public static TaskManagerTask ScaleOpen(RectTransform rectTransform, AnimationCurve curve, float timeMultiplier = 1)
        {
            return new TaskManagerTask(ScaleCurveOpenCoroutine(rectTransform, curve, timeMultiplier), true);
        }

        public static TaskManagerTask ScaleClose(RectTransform rectTransform, float time)
        {
            return new TaskManagerTask(ScaleTimeCloseCoroutine(rectTransform, time), true);
        }

        public static TaskManagerTask ScaleClose(RectTransform rectTransform, AnimationCurve curve, float timeMultiplier = 1)
        {
            return new TaskManagerTask(ScaleCurveCloseCoroutine(rectTransform, curve, timeMultiplier), true);
        }

        static IEnumerator ScaleTimeOpenCoroutine(RectTransform rectTransform, float time)
        {
            float delta = 0;

            while(rectTransform.localScale.x < 1)
            {
                delta = Time.deltaTime/time;
                rectTransform.localScale += new Vector3(delta, delta, 0);
                yield return null;
            }

            rectTransform.localScale = new Vector3(1, 1, 1);
        }

        static IEnumerator ScaleTimeCloseCoroutine(RectTransform rectTransform, float time)
        {
            float delta = 0;

            while(rectTransform.localScale.x > 0)
            {
                delta = Time.deltaTime/time;
                rectTransform.localScale -= new Vector3(delta, delta, 0);
                yield return null;
            }

            rectTransform.localScale = new Vector3(0, 0, 1);
        }

        static IEnumerator ScaleCurveOpenCoroutine(RectTransform rectTransform, AnimationCurve curve, float timeMultiplier = 1)
        {
            float time = 0;
            float value = 0;
            while(rectTransform.localScale.x < 1)
            {
                value = curve.Evaluate(time);
                rectTransform.localScale = new Vector3(value, value, 0);
                time += Time.deltaTime/timeMultiplier;
                yield return null;
            }

            rectTransform.localScale = new Vector3(1, 1, 1);
        }

        static IEnumerator ScaleCurveCloseCoroutine(RectTransform rectTransform, AnimationCurve curve, float timeMultiplier = 1)
        {
            float time = 0;
            float value = 0;
            while(rectTransform.localScale.x > 0)
            {
                value = curve.Evaluate(time);
                rectTransform.localScale = new Vector3(value, value, 0);
                time += Time.deltaTime/timeMultiplier;
                yield return null;
            }

            rectTransform.localScale = new Vector3(0, 0, 1);
        }
    }
}