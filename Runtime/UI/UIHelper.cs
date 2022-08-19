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
        public static Dictionary<Transform, TaskManagerTask> TaskDict = new Dictionary<Transform, TaskManagerTask>();

        public static bool ScaleOpen(Transform transform, float time, bool force = false)
        {
            TaskManagerTask task;
            if (TaskDict.TryGetValue(transform, out task))
            {
                if (!force) return false;
                task.Stop();
            } 

            CoroutineHelper.OnNextFrame(() => 
            {
                task = new TaskManagerTask(ScaleTimeOpenCoroutine(transform, time), true);
                TaskDict.Add(transform, task);
                task.Finished += (stoped) => AfterTaskFinish(transform, stoped);
            });
            return true;
        }

        public static bool ScaleOpen(Transform transform, AnimationCurve curve = null, float timeMultiplier = 1, bool force = false)
        {
            TaskManagerTask task;
            if (TaskDict.TryGetValue(transform, out task))
            {
                if (!force) return false;
                task.Stop();
            } 

            CoroutineHelper.OnNextFrame(() => 
            {
                if (curve == null) task = new TaskManagerTask(ScaleCurveOpenCoroutine(transform, _defaultCurve, timeMultiplier), true);
                else task =  new TaskManagerTask(ScaleCurveOpenCoroutine(transform, curve, timeMultiplier), true);
                TaskDict.Add(transform, task);
                task.Finished += (stoped) => AfterTaskFinish(transform, stoped);
            });
            return true;
        }

        public static bool ScaleClose(Transform transform, float time, bool force = false)
        {
            TaskManagerTask task;
            if (TaskDict.TryGetValue(transform, out task))
            {
                if (!force) return false;
                task.Stop();
            }

            CoroutineHelper.OnNextFrame(() =>
            {
                task = new TaskManagerTask(ScaleTimeCloseCoroutine(transform, time), true);
                TaskDict.Add(transform, task);
                task.Finished += (stoped) => AfterTaskFinish(transform, stoped);
            });
            return true;
        }

        public static bool ScaleClose(Transform transform, AnimationCurve curve = null, float timeMultiplier = 1, bool force = false)
        {
            TaskManagerTask task;
            if (TaskDict.TryGetValue(transform, out task))
            {
                if (!force) return false;
                task.Stop();
            }

            CoroutineHelper.OnNextFrame(() =>
            {
                if (curve == null) task = new TaskManagerTask(ScaleCurveCloseCoroutine(transform, AnimationCurveHelper.ReverseCurve(_defaultCurve), timeMultiplier), true);
                else task = new TaskManagerTask(ScaleCurveCloseCoroutine(transform, curve, timeMultiplier), true);
                TaskDict.Add(transform, task);
                task.Finished += (stoped) => AfterTaskFinish(transform, stoped);
            });
            return true;
        }

        static IEnumerator ScaleTimeOpenCoroutine(Transform transform, float time)
        {
            float delta = 0;

            while(transform.localScale.x < 1)
            {
                delta = Time.deltaTime/time;
                transform.localScale += new Vector3(delta, delta, 0);
                yield return null;
            }

            transform.localScale = new Vector3(1, 1, 1);
        }

        static IEnumerator ScaleTimeCloseCoroutine(Transform transform, float time)
        {
            float delta = 0;

            while(transform.localScale.x > 0)
            {
                delta = Time.deltaTime/time;
                transform.localScale -= new Vector3(delta, delta, 0);
                yield return null;
            }

            transform.localScale = new Vector3(0, 0, 1);
        }

        static IEnumerator ScaleCurveOpenCoroutine(Transform transform, AnimationCurve curve, float timeMultiplier = 1)
        {
            float time = 0;
            float value = 0;
            while(transform.localScale.x < 1)
            {
                value = curve.Evaluate(time);
                transform.localScale = new Vector3(value, value, 1);
                time += Time.deltaTime/timeMultiplier;
                yield return null;
            }

            transform.localScale = new Vector3(1, 1, 1);
        }

        static IEnumerator ScaleCurveCloseCoroutine(Transform transform, AnimationCurve curve, float timeMultiplier = 1)
        {
            float time = 0;
            float value = 0;
            while(transform.localScale.x > 0)
            {
                value = curve.Evaluate(time);
                transform.localScale = new Vector3(value, value, 1);
                time += Time.deltaTime/timeMultiplier;
                yield return null;
            }

            transform.localScale = new Vector3(0, 0, 1);
        }

        static void AfterTaskFinish(Transform transform, bool stoped)
        {
            if (TaskDict.ContainsKey(transform))
            {
                TaskDict.Remove(transform);
            }
        }
    }
}