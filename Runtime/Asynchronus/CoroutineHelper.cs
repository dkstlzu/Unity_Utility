using System;
using System.Collections;

namespace dkstlzu.Utility
{
    public class CoroutineHelper : Singleton<CoroutineHelper>
    {
        public static void OnNextFrame(Action action)
        {
            CoroutineHelper.GetOrCreateInstance.StartCoroutine(NextFrame(action));
        }

        static IEnumerator NextFrame(Action action)
        {
            yield return null;
            action.Invoke();
        }
    }
}