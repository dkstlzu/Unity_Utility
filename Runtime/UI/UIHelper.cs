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
        public static Dictionary<RectTransform, TaskManagerTask> TaskDict = new Dictionary<RectTransform, TaskManagerTask>();

        public static bool ScaleOpen(RectTransform rectTransform, float time, bool force = false)
        {
            TaskManagerTask task;
            if (TaskDict.TryGetValue(rectTransform, out task))
            {
                if (!force) return false;
                task.Stop();
            } 

            CoroutineHelper.OnNextFrame(() => 
            {
                task = new TaskManagerTask(ScaleTimeOpenCoroutine(rectTransform, time), true);
                TaskDict.Add(rectTransform, task);
                task.Finished += (stoped) => AfterTaskFinish(rectTransform, stoped);
            });
            return true;
        }

        public static bool ScaleOpen(RectTransform rectTransform, AnimationCurve curve = null, float timeMultiplier = 1, bool force = false)
        {
            TaskManagerTask task;
            if (TaskDict.TryGetValue(rectTransform, out task))
            {
                if (!force) return false;
                task.Stop();
            } 

            CoroutineHelper.OnNextFrame(() => 
            {
                if (curve == null) task = new TaskManagerTask(ScaleCurveOpenCoroutine(rectTransform, _defaultCurve, timeMultiplier), true);
                else task =  new TaskManagerTask(ScaleCurveOpenCoroutine(rectTransform, curve, timeMultiplier), true);
                TaskDict.Add(rectTransform, task);
                task.Finished += (stoped) => AfterTaskFinish(rectTransform, stoped);
            });
            return true;
        }

        public static bool ScaleClose(RectTransform rectTransform, float time, bool force = false)
        {
            TaskManagerTask task;
            if (TaskDict.TryGetValue(rectTransform, out task))
            {
                if (!force) return false;
                task.Stop();
            }

            CoroutineHelper.OnNextFrame(() =>
            {
                task = new TaskManagerTask(ScaleTimeCloseCoroutine(rectTransform, time), true);
                TaskDict.Add(rectTransform, task);
                task.Finished += (stoped) => AfterTaskFinish(rectTransform, stoped);
            });
            return true;
        }

        public static bool ScaleClose(RectTransform rectTransform, AnimationCurve curve = null, float timeMultiplier = 1, bool force = false)
        {
            TaskManagerTask task;
            if (TaskDict.TryGetValue(rectTransform, out task))
            {
                if (!force) return false;
                task.Stop();
            }

            CoroutineHelper.OnNextFrame(() =>
            {
                if (curve == null) task = new TaskManagerTask(ScaleCurveCloseCoroutine(rectTransform, AnimationCurveHelper.ReverseCurve(_defaultCurve), timeMultiplier), true);
                else task = new TaskManagerTask(ScaleCurveCloseCoroutine(rectTransform, curve, timeMultiplier), true);
                TaskDict.Add(rectTransform, task);
                task.Finished += (stoped) => AfterTaskFinish(rectTransform, stoped);
            });
            return true;
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

        static void AfterTaskFinish(RectTransform rectTransform, bool stoped)
        {
            if (TaskDict.ContainsKey(rectTransform))
            {
                TaskDict.Remove(rectTransform);
            }
        }
    }
}