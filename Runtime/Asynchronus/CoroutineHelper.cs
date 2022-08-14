using System;
using System.Collections;

using UnityEngine;

namespace dkstlzu.Utility
{
    public class CoroutineHelper : Singleton<CoroutineHelper>
    {
        public static void OnNextFrame(Action action)
        {
            CoroutineHelper.GetOrCreateInstance.StartCoroutine(AfterFrame(action, 1));
        }

        public static void Delay(Action action, float time)
        {
            CoroutineHelper.GetOrCreateInstance.StartCoroutine(AfterTime(action, time));
        }
        
        static IEnumerator AfterFrame(Action action, int frame)
        {
            while(frame-- > 0)
            {
                yield return null;
            }
            action.Invoke();
        }

        static IEnumerator AfterTime(Action action, float time)
        {
            yield return new WaitForSeconds(time);
            action.Invoke();
        }
    }
}