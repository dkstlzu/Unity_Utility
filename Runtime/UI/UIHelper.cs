using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace dkstlzu.Utility
{
    public class UIHelper
    {
        static AnimationCurve _defaultCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        private static Dictionary<RectTransform, TaskManagerTask> _taskDict = new Dictionary<RectTransform, TaskManagerTask>();

        public static TaskManagerTask ScaleOpen(RectTransform rectTransform, float time)
        {
            TaskManagerTask task;
            if (_taskDict.TryGetValue(rectTransform, out task))
            {
                task.Stop();
            }
            task = new TaskManagerTask(ScaleTimeOpenCoroutine(rectTransform, time), true);
            return task;
        }

        public static TaskManagerTask ScaleOpen(RectTransform rectTransform, AnimationCurve curve = null, float timeMultiplier = 1)
        {
            TaskManagerTask task;
            if (_taskDict.TryGetValue(rectTransform, out task))
            {
                task.Stop();
            }
            if (curve == null) task = new TaskManagerTask(ScaleCurveOpenCoroutine(rectTransform, _defaultCurve, timeMultiplier), true);
            else task =  new TaskManagerTask(ScaleCurveOpenCoroutine(rectTransform, curve, timeMultiplier), true);
            return task;
        }

        public static TaskManagerTask ScaleClose(RectTransform rectTransform, float time)
        {
            TaskManagerTask task;
            if (_taskDict.TryGetValue(rectTransform, out task))
            {
                task.Stop();
            }
            task = new TaskManagerTask(ScaleTimeCloseCoroutine(rectTransform, time), true);
            return task;
        }

        public static TaskManagerTask ScaleClose(RectTransform rectTransform, AnimationCurve curve = null, float timeMultiplier = 1)
        {
            TaskManagerTask task;
            if (_taskDict.TryGetValue(rectTransform, out task))
            {
                task.Stop();
            }
            if (curve == null) task = new TaskManagerTask(ScaleCurveCloseCoroutine(rectTransform, AnimationCurveHelper.ReverseCurve(_defaultCurve), timeMultiplier), true);
            task = new TaskManagerTask(ScaleCurveCloseCoroutine(rectTransform, curve, timeMultiplier), true);
            return task;
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
                rectTransform.localScale = new Vector3(value, value, 1);
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
                rectTransform.localScale = new Vector3(value, value, 1);
                time += Time.deltaTime/timeMultiplier;
                yield return null;
            }

            rectTransform.localScale = new Vector3(0, 0, 1);
        }
    }
}