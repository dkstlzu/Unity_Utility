using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if USING_URP
using UnityEngine.Rendering.Universal;
#endif

namespace dkstlzu.Utility
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
        private Camera fadeCamera;
        private bool autoDispose;
        
        void Update()
        {
            if (!started) return;
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
                EndFade();
                if (Alpha <= 0) isFadedIn = true;
                else isFadedOut = true;
            }
        }
        
        /// <param name="sortingOrder">Canvas Sorting Order that can be at most frontside</param>
        public static FadeEffect Init(int sortingOrder = 100, bool autoDispose = true)
        {
            GameObject CameraGO = new GameObject("FadeEffect Camera");
            Camera FadeCam = CameraGO.AddComponent<Camera>();

            GameObject fadeEffectGO = new GameObject("FadeEffect", new Type[]{typeof(FadeEffect), typeof(Canvas)});
            Canvas FadeCanvas = fadeEffectGO.GetComponent<Canvas>();
            fadeEffectGO.transform.SetParent(CameraGO.transform);
            fadeEffectGO.layer = 1;
            
            GameObject ImageGO = new GameObject("Image",  new Type[]{typeof(Image)});
            ImageGO.transform.SetParent(fadeEffectGO.transform);
            Image image = ImageGO.GetComponent<Image>();
            RectTransform ImageRect = ImageGO.GetComponent<RectTransform>();
            FadeEffect fadeEffect = fadeEffectGO.GetComponent<FadeEffect>();
            fadeEffect.autoDispose = autoDispose;
            
            fadeEffect.FadeImage = image;
            image.color = new Color(0, 0, 0, 0);
            ImageRect.anchorMin = Vector2.zero;
            ImageRect.anchorMax = Vector2.one;
            
            fadeEffect.fadeCamera = FadeCam;
            fadeEffect.fadeCamera.orthographic = true;
            fadeEffect.fadeCamera.orthographicSize = Camera.main.orthographicSize;
            fadeEffect.fadeCamera.cullingMask = 2;
            fadeEffect.fadeCamera.transform.position = Camera.main.transform.position;

            FadeCanvas.sortingOrder = sortingOrder;
            FadeCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            FadeCanvas.worldCamera = fadeEffect.fadeCamera;

#if USING_URP
            UniversalAdditionalCameraData data = fadeEffect.fadeCamera.GetUniversalAdditionalCameraData();
            data.renderType = CameraRenderType.Overlay;
            UniversalAdditionalCameraData cameraData = Camera.main.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Add(fadeEffect.fadeCamera);
#endif
            return fadeEffect;
        }

        void EndFade()
        {
            isFadingIn = false;
            isFadingOut = false;
            if (autoDispose) Dispose();
        }

        public void Stop()
        {
            started = false;
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

        public void LoadSceneWithFadeEffect(string scene, float duration, LoadSceneMode mode = LoadSceneMode.Single)
        {
            new TaskManagerTask(LoadSceneAfter(scene, duration, mode), true);
        }

        IEnumerator LoadSceneAfter(string scene, float duration, LoadSceneMode mode = LoadSceneMode.Single)
        {
            bool disposeBank = autoDispose;
            autoDispose = false;

            Out(duration);
            yield return new WaitForSeconds(duration);
            int sortingOrder = GetComponent<Canvas>().sortingOrder;
            SceneManager.LoadSceneAsync(scene, mode).completed += (ar) => FadeEffect.Init(sortingOrder, autoDispose: disposeBank).In(duration);
        }

        public void Dispose()
        {
#if USING_URP
            Camera.main.GetUniversalAdditionalCameraData().cameraStack.Remove(fadeCamera);
#endif
            Destroy(fadeCamera.gameObject);
        }
    }
}