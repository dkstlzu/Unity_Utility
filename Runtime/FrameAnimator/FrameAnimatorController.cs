using System;
using UnityEngine;

namespace dkstlzu.Utility
{
    public class FrameAnimatorController : MonoBehaviour
    {
        public FrameAnimator Animator;

        public bool Loop
        {
            get => Animator.LoopCurrent;
            set => Animator.LoopCurrent = value;
        }

        public bool AutoTransition
        {
            get => Animator.AutoTransition;
            set => Animator.AutoTransition = value;
        }

        public int DefaultAnimationIndex
        {
            get => Animator.DefaultAnimationIndex;
            set => Animator.DefaultAnimationIndex = value;
        }

        public bool IsRunning => Animator.IsRunning;

        private void Awake()
        {
            Animator.Init();
        }

        private void OnEnable()
        {
            DefaultFrameCountUpdateManager.Instance?.Register(Animator, -1);
        }

        private void OnDisable()
        {
            DefaultFrameCountUpdateManager.Instance?.Unregister(Animator, -1);
        }

        public void PlayAnimation(string animationName) => Animator.PlayAnimation(animationName);
        public void PlayAnimation(int animationIndex) => Animator.PlayAnimation(animationIndex);
        public void PlayFrameOffset(int frameNum) => Animator.PlayFrame(Animator.CurrentFrame + frameNum);
        public void Replay() => Animator.Replay();
        public void Run() => Animator.Run();
        public void Stop() => Animator.Stop();
        public void Pause() => Animator.Pause();
        public void Unpause() => Animator.Unpause();
        public void Delay(int delayFrameNum) => Animator.Delay(delayFrameNum);
        public void AddEnterEvent(string animationName, Action action) => Animator.AddEnterEvent(animationName, action);
        public void RemoveEnterEvent(string animationName, Action action) => Animator.RemoveEnterEvent(animationName, action);
        public void AddStayEvent(string animationName, Action action) => Animator.AddStayEvent(animationName, action);
        public void RemoveStayEvent(string animationName, Action action) => Animator.RemoveStayEvent(animationName, action);
        public void AddExitEvent(string animationName, Action action) => Animator.AddExitEvent(animationName, action);
        public void RemoveExitEvent(string animationName, Action action) => Animator.RemoveExitEvent(animationName, action);
        public void AddFrameEvent(string animationName, int frame, Action action) => Animator.AddFrameEvent(animationName, frame, action);
        public void RemoveFrameEvent(string animationName, int frame, Action action) => Animator.RemoveFrameEvent(animationName, frame, action);
    }
}