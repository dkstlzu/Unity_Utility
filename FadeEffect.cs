using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Utility
{
    public class FadeEffect : MonoBehaviour, IDisposable
    {
        private float Duration;
        public float Alpha;
        public bool isFadingIn;
        public bool isFadingOut;
        public bool isFadedIn;
        public bool isFadedOut;
        public Image FadeImage;
        private bool started;
        
        void Update()
        {
            if (!isFadingIn && !isFadingOut) return;

            if (isFadingIn)
            {
                Alpha -= Time.deltaTime / Duration;
            } else if (isFadingOut)
            {
                Alpha += Time.deltaTime / Duration;
            }

            FadeImage.color = new Color(0, 0, 0, Alpha);

            if (Alpha <= 0 || Alpha >= 1)
            {
                Alpha = Mathf.Clamp01(Alpha);
                Reset();
                if (Alpha <= 0) isFadedIn = true;
                else isFadedOut = true;
            }
        }
        
        /// <param name="sortingOrder">Canvas Sorting Order that can be at most frontside</param>
        public static FadeEffect Init(int sortingOrder)
        {
            Type[] types = new Type[]{typeof(Canvas), typeof(CanvasScaler)};
            GameObject CanvasGO;
            (CanvasGO = new GameObject("FadeEffect Canvas", types)).transform.SetParent(Camera.main.transform);
            CanvasGO.GetComponent<Canvas>().sortingOrder = sortingOrder;
            FadeEffect fadeEffect = CanvasGO.AddComponent<FadeEffect>();
            GameObject FadeImageGO;
            (FadeImageGO = new GameObject("FadeImage")).transform.SetParent(CanvasGO.transform);
            fadeEffect.FadeImage = FadeImageGO.AddComponent<Image>();
            fadeEffect.FadeImage.color = new Color(0, 0, 0, 0);
            fadeEffect.Alpha = 1;
            return fadeEffect;
        }

        public void Reset()
        {
            isFadingIn = false;
            isFadingOut = false;
            // GetComponent<Canvas>().sortingOrder *= -1; 
        }

        public void In(float duration)
        {
            if (!started)
            {
                Alpha = 1;
                started = true;
            }
            Duration = duration;
            isFadingIn = true;
            isFadingOut = false;
            isFadedIn = false;
            isFadedOut = false;
        }

        public void Out(float duration)
        {
            if (!started)
            {
                Alpha = 0;
                started = true;
            }
            Duration = duration;
            isFadingIn = false;
            isFadingOut = true;
            isFadedIn = false;
            isFadedOut = false;
        }

        public async void LoadSceneWithFadeEffect(string scene, float duration, LoadSceneMode mode = LoadSceneMode.Single)
        {
            Out(duration);
            await Task.Delay((int)(duration * 1000));
            int sortingOrder = GetComponent<Canvas>().sortingOrder;
            SceneManager.LoadSceneAsync(scene, mode).completed += (ar) => FadeEffect.Init(sortingOrder).In(duration);
        }

        public void Dispose()
        {
            Destroy(gameObject);
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("DevTest/FadeTest/In")]
        static void DevTestIn()
        {
            FadeEffect.Init(100).In(3);
        }

        [UnityEditor.MenuItem("DevTest/FadeTest/Out")]
        static void DevTestOut()
        {
            FadeEffect.Init(100).Out(3);
        }
#endif
    }
}