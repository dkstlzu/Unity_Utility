using System;
using System.Collections;
using System.Threading;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace dkstlzu.Utility.Demo
{
    public class TempGameManager : MonoBehaviour
    {
        public FrameAnimatorController Controller;
        
        public int FrameRate;
        
        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 9999;
            StartCoroutine(WaitForNextFrame());
        }

        private void Start()
        {
            Controller.AddEnterEvent("Walk", () => Printer.Print("Walk Enter"));
            Controller.AddExitEvent("Walk", () => Printer.Print("Walk Exit"));
            Controller.AddStayEvent("Walk", () => Printer.Print($"Walking {Controller.Animator.CurrentFrame}"));
            Controller.AddEnterEvent("Attack1", () => Printer.Print("Attack1 Enter"));
            Controller.AddExitEvent("Attack1", () => Printer.Print("Attack1 Exit"));
            
            Controller.AddFrameEvent("Walk", 10, () => Printer.Print($"Animation Name {Controller.Animator.CurrentAnimationName}, Frame {Controller.Animator.CurrentFrame}"));
        }

        /// <summary>
        /// 링크를 참조하여 만듬
        /// "https://blog.unity.com/engine-platform/precise-framerates-in-unity"
        /// </summary>
        IEnumerator WaitForNextFrame()
        {
            float currentFrameTime = Time.realtimeSinceStartup;
            
            while (true)
            {
                yield return new WaitForEndOfFrame();
                currentFrameTime += 1.0f / FrameRate;
                var t = Time.realtimeSinceStartup;
                var sleepTime = currentFrameTime - t - 0.01f;
                if (sleepTime > 0)
                    Thread.Sleep((int)(sleepTime * 1000));
                while (t < currentFrameTime)
                    t = Time.realtimeSinceStartup;
            }
        }

        private void Update()
        {
            // if (Application.targetFrameRate != FrameRate)
            // {
            //     Application.targetFrameRate = FrameRate;
            // }

            #region movement

#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            
            if (kb.aKey.wasPressedThisFrame)
            {
                Controller.GetComponentInChildren<SpriteRenderer>().flipX = true;
                Controller.PlayAnimation("walk");
            } else if ((kb.dKey.wasPressedThisFrame))
            {
                Controller.GetComponentInChildren<SpriteRenderer>().flipX = false;
                Controller.PlayAnimation("walk");
            } else if ((kb.spaceKey.wasPressedThisFrame))
            {
                Controller.PlayAnimation("Attack1");
            }

            if ((kb.aKey.wasReleasedThisFrame && !kb.anyKey.isPressed) || (kb.dKey.wasReleasedThisFrame && !kb.anyKey.isPressed))
            {
                Controller.PlayAnimation(Controller.DefaultAnimationIndex);
            }

            Controller.Loop = Keyboard.current.anyKey.isPressed;
#else
            if (Input.GetKeyDown(KeyCode.A))
            {
                Controller.GetComponentInChildren<SpriteRenderer>().flipX = true;
                Controller.PlayAnimation("walk");
            } else if (Input.GetKeyDown(KeyCode.D))
            {
                Controller.GetComponentInChildren<SpriteRenderer>().flipX = false;
                Controller.PlayAnimation("walk");
            } else if (Input.GetKeyDown(KeyCode.Space))
            {
                Controller.PlayAnimation("Attack1");
            }

            if ((Input.GetKeyUp(KeyCode.A) && !Input.anyKey) || (Input.GetKeyUp(KeyCode.D) && !Input.anyKey))
            {
                Controller.PlayAnimation(Controller.DefaultAnimationIndex);
            }

            Controller.Loop = Input.anyKey;
#endif

            #endregion

            #region Frame managing

#if ENABLE_INPUT_SYSTEM
            if (kb.tKey.wasPressedThisFrame)
            {
                Controller.Delay(50);
            } else if (kb.pKey.wasPressedThisFrame)
            {
                if (Controller.IsRunning) Controller.Pause();
                else Controller.Unpause();
            } else if (kb.oKey.wasPressedThisFrame)
            {
                if (Controller.IsRunning) Controller.Stop();
                else Controller.Run();
            } else if (kb.iKey.wasPressedThisFrame)
            {
                Controller.PlayFrameOffset(15);
            }
#else
            if (Input.GetKeyDown(KeyCode.T))
            {
                Controller.Delay(50);
            } else if (Input.GetKeyDown(KeyCode.P))
            {
                if (Controller.IsRunning) Controller.Pause();
                else Controller.Unpause();
            } else if (Input.GetKeyDown(KeyCode.O))
            {
                if (Controller.IsRunning) Controller.Stop();
                else Controller.Run();
            } else if (Input.GetKeyDown(KeyCode.I))
            {
                Controller.PlayFrameOffset(15);
            }
#endif

            #endregion
        }
        
        private void OnGUI()
        {
            GUI.skin.label.fontSize = 20;
            GUI.Label(new Rect(20, 20, 200, 50), $"Frame {Time.frameCount}");
            GUI.Label(new Rect(20, 70, 200, 50), $"Time {Time.timeSinceLevelLoad}");
            GUI.Label(new Rect(20, 100, 200, 50), $"FrameRate - {FrameRate}");
            FrameRate = (int)GUI.Slider(new Rect(20, 120, 200, 50), FrameRate, 10, 0, 200, GUI.skin.horizontalSlider,
                GUI.skin.horizontalSliderThumb, true, 0);
        }
    }
}