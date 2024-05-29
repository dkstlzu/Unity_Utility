using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if USING_URP
using UnityEngine.Rendering.Universal;
#endif

namespace dkstlzu.Utility
{
    public class FadeEffect : MonoBehaviour
    {
        public void LoadSceneWithFadeEffect(string scene, float duration, LoadSceneMode mode = LoadSceneMode.Single)
        {
            StartCoroutine(LoadSceneAfter(scene, duration, mode));
        }

        IEnumerator LoadSceneAfter(string scene, float duration, LoadSceneMode mode = LoadSceneMode.Single)
        {
            bool disposeBank = AutoDispose;
            AutoDispose = false;

            Out(duration);
            yield return new WaitForSeconds(duration);
            SceneManager.LoadSceneAsync(scene, mode).completed += (ar) => In(duration);
            AutoDispose = disposeBank;
        }
        
        /// <param name="sortingOrder">Canvas Sorting Order that can be at most frontside</param>
        public static FadeEffect Init(int sortingOrder = 100, bool autoDispose = true)
        {
            GameObject cameraGo = new GameObject("FadeEffect Camera");
            DontDestroyOnLoad(cameraGo);
            Camera fadeCam = cameraGo.AddComponent<Camera>();

            GameObject fadeEffectGo = new GameObject("FadeEffect", new Type[]{typeof(FadeEffect), typeof(Canvas)});
            FadeEffect fadeEffect = fadeEffectGo.GetComponent<FadeEffect>();
            fadeEffect.FadeCanvas = fadeEffectGo.GetComponent<Canvas>();
            fadeEffectGo.transform.SetParent(cameraGo.transform);
            fadeEffectGo.layer = 1;
            
            GameObject imageGo = new GameObject("Image",  new Type[]{typeof(Image)});
            imageGo.transform.SetParent(fadeEffectGo.transform);
            Image image = imageGo.GetComponent<Image>();
            RectTransform imageRect = imageGo.GetComponent<RectTransform>();
            fadeEffect.AutoDispose = autoDispose;
            
            fadeEffect.FadeImage = image;
            image.color = new Color(0, 0, 0, 0);
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            
            fadeEffect.FadeCamera = fadeCam;
            fadeEffect.FadeCamera.orthographic = true;
            fadeEffect.FadeCamera.orthographicSize = Camera.main.orthographicSize;
            fadeEffect.FadeCamera.cullingMask = 2;
            fadeEffect.FadeCamera.transform.position = Camera.main.transform.position;

            fadeEffect.FadeCanvas.sortingOrder = sortingOrder;
            fadeEffect.FadeCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            fadeEffect.FadeCanvas.worldCamera = fadeEffect.FadeCamera;

#if USING_URP
            UniversalAdditionalCameraData data = fadeEffect.fadeCamera.GetUniversalAdditionalCameraData();
            data.renderType = CameraRenderType.Overlay;
            UniversalAdditionalCameraData cameraData = Camera.main.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Add(fadeEffect.fadeCamera);
#endif
            return fadeEffect;
        }
                
        public float Duration;
        public float Alpha;
        public bool Started;
        public bool IsFadingIn;
        public bool IsFadingOut;
        public bool IsFadedIn => Alpha >= 1;
        public bool IsFadedOut => Alpha <= 0;
        public Image FadeImage;
        
        public Camera FadeCamera;
        public Canvas FadeCanvas;
        public bool AutoDispose;
        
        void Update()
        {
            if (!Started) return;

            if (IsFadingIn)
            {
                Alpha -= Time.deltaTime / Duration;
            } else if (IsFadingOut)
            {
                Alpha += Time.deltaTime / Duration;
            }

            FadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(Alpha));
            if (Alpha < 0 || Alpha > 1)
            {
                Stop();
            }
        }
        
        public void In(float duration)
        {
            if (!Started)
            {
                Alpha = 1;
                Started = true;
            }
            Duration = duration;
            IsFadingIn = true;
            IsFadingOut = false;
        }

        public void Out(float duration)
        {
            if (!Started)
            {
                Alpha = 0;
                Started = true;
            }
            Duration = duration;
            IsFadingIn = false;
            IsFadingOut = true;
        }
        
        public void Stop()
        {
            Started = false;
            IsFadingIn = false;
            IsFadingOut = false;

            if (AutoDispose)
            {
                Dispose();
            }
        }

        public void Dispose()
        {
#if USING_URP
            Camera.main.GetUniversalAdditionalCameraData().cameraStack.Remove(fadeCamera);
#endif
            Destroy(FadeCamera.gameObject);
        }
    }
}