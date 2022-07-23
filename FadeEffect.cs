using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Utility
{
    public class FadeEffect : MonoBehaviour, IDisposable
    {
        private float Duration;
        public float Alpha;
        public bool isFadingIn;
        public bool isFadingOut;
        public Image FadeImage;
        
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
            }
        }
        
        /// <param name="sortingOrder">Canvas Sorting Order that can be at most frontside</param>
        public static FadeEffect Init(int sortingOrder)
        {
            Type[] types = new Type[]{typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)};
            GameObject CanvasGO;
            (CanvasGO = new GameObject("FadeEffect Canvas", types)).transform.SetParent(Camera.main.transform);
            CanvasGO.GetComponent<Canvas>().sortingOrder = sortingOrder;
            FadeEffect fadeEffect = CanvasGO.AddComponent<FadeEffect>();
            GameObject FadeImageGO;
            (FadeImageGO = new GameObject("FadeImage")).transform.SetParent(CanvasGO.transform);
            fadeEffect.FadeImage = FadeImageGO.AddComponent<Image>();
            fadeEffect.FadeImage.color = new Color(0, 0, 0, 1);
            fadeEffect.Alpha = 1;
            return fadeEffect;
        }

        public void Reset()
        {
            isFadingIn = false;
            isFadingOut = false;
            GetComponent<Canvas>().sortingOrder *= -1; 
        }

        public void In(float time)
        {
            Duration = time;
            isFadingIn = true;
            isFadingOut = false;
        }

        public void Out(float time)
        {
            Duration = time;
            isFadingIn = false;
            isFadingOut = true;
        }

        public void Dispose()
        {
            Destroy(gameObject);
        }
    }
}