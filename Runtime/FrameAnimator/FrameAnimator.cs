using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace dkstlzu.Utility
{
    [Serializable]
    public class FrameAnimator : IFrameCountUpdatable, ISerializationCallbackReceiver
    {
        [Serializable]
        class AnimationElement : IFrameCountUpdatable
        {
            public string Name;
            [SerializeField]
            private FrameAnimationInfoScriptable _animationInfo;
            public Sprite[] Sprites => _animationInfo.Sprites.Sequences;
            
            [SerializeField, HideInInspector]
            private int _currentFrame = 0;
            public int CurrentFrame {
                get => _currentFrame;
                set
                {
                    if (!IsValidFrame(value))
                    {
                        // TODO : 여기에서 그냥 return하는 것이 아니라 다음 애니메이션으로의 점프 기능이 들어가야합니다.
                        return;
                    }
                    
                    _currentFrame = value;
                    CurrentSpriteIndex = _animationInfo.GetSpriteIndexOfFrame(CurrentFrame);
                    UpdateRenderer();
                }
            }

            [SerializeField, HideInInspector]
            private int _currentSpriteIndex;
            public int CurrentSpriteIndex
            {
                get => _currentSpriteIndex;
                private set => _currentSpriteIndex = value;
            }

            public Sprite CurrentSprite => _animationInfo.Sprites.Sequences[CurrentSpriteIndex];
            public int TotalFrameNum => _animationInfo.TotalFrameNum;

            [SerializeField, HideInInspector]
            private SpriteRenderer _renderer;

            public void SetTargetRenderer(SpriteRenderer renderer)
            {
                _renderer = renderer;
            }

            public bool IsUpdatable(int frame)
            {
                return IsValidFrame(CurrentFrame + frame);
            }

            public void FrameCountUpdate(int frame)
            {
                Assert.IsNotNull(_renderer, "Animation Target renderer is not set yet.");

                if (CurrentFrame >= _animationInfo.TotalFrameNum)
                {
                    return;
                }

                if (IsUpdatable(frame))
                {
                    CurrentFrame += frame;
                }
                else
                {
                    CurrentFrame = _animationInfo.TotalFrameNum;
                }
            }

            public void UpdateRenderer()
            {
                _renderer.sprite = CurrentSprite;
            }

            public void Reset()
            {
                CurrentFrame = 0;
            }

            public Sprite GetSpriteOfFrame(int frame) => _animationInfo.GetSpriteOfFrame(frame);
            public int GetFrameOfSprite(int spriteIndex) => _animationInfo.GetFrameOfSpriteIndex(spriteIndex);
            public ReadOnlySpan<Rect> GetBodyBoxes(int frame) => _animationInfo.GetBodyRectsOfFrame(frame);
            public ReadOnlySpan<Rect> GetHitBoxes(int frame) => _animationInfo.GetHitRectsOfFrame(frame);
            public ReadOnlySpan<Rect> GetAttackBoxes(int frame) => _animationInfo.GetAttackRectsOfFrame(frame);

            private bool IsValidFrame(int frame)
            {
                return frame >= 0 && frame <= _animationInfo.TotalFrameNum;
            }
        }

        public int DefaultAnimationIndex;
        public bool PlayOnAwake;
        public bool LoopCurrent;
        public bool AutoTransition;
        
        [SerializeField]
        private List<AnimationElement> _animations;

        [SerializeField] 
        private SpriteRenderer _renderer;


        #region Main Logic

        private int _previousFrame;
        private int _previousAnimationIndex;
        private bool _animationIsCompletedOnThisFrame;
        
        private int _delayFrame = 0;

        public void Init()
        {
            TotalFrameSinceInit = 0;
            
            foreach (var element in _animations)
            {
                element.SetTargetRenderer(_renderer);
            }

            if (PlayOnAwake)
            {
                Run();
            }
        }

        public void FrameCountUpdate(int frame)
        {
            if (!_isRunning)
            {
                return;
            }

            if (_delayFrame > 0)
            {
                _delayFrame--;
                return;
            }

            TotalFrameSinceInit += frame;
            _previousFrame = CurrentFrame;
            _previousAnimationIndex = _currentAnimationIndex;

            if (_currentAnimationElement.IsUpdatable(frame))
            {
                _currentAnimationElement.FrameCountUpdate(frame);
                _animationIsCompletedOnThisFrame = false;
            } else if (LoopCurrent)
            {
                Replay();
                _animationIsCompletedOnThisFrame = true;
            } else if (AutoTransition)
            {
                // TODO
                // 여기서 프레임 손상이 있습니다.
                // 만약 3프레임의 애니메이션이 남아있고 6프레임 만큼을 업데이트하고자 할 경우
                // 3프레임을 이전 애니메이션에서 사용하고 남은 3프레임을 다음 애니메이션에서 처리하여
                // 다음 애니메이션을 3프레임부터 시작해야하지만 현재는 구현이 안되어있습니다.
                PlayAnimation(DefaultAnimationIndex);
                _animationIsCompletedOnThisFrame = true;
            } else
            {
                // Do Nothing
            }
            
            _sm.FrameUpdate(default);
        }
        #endregion
        
        #region Playing sequence

        private bool _isRunning = false;
        public bool IsRunning => _isRunning;
        
        public FrameAnimator PlayAnimation(string animationName)
        {
            if (tryFindAnimationElementIndex(animationName, out int animationIndex))
            {
                return PlayAnimation(animationIndex);
            }

            return this;
        }

        public FrameAnimator PlayAnimation(int animationIndex)
        {
            Assert.IsTrue(IsValidAnimationIndex(animationIndex), "잘못된 animation 번호입니다.");
            
            _currentAnimationElement.Reset();
            _currentAnimationIndex = animationIndex;
            _sm.ChangeTo(GetAnimationName(animationIndex), false);
            _currentAnimationElement.Reset();
            
            return this;
        }
        
        public FrameAnimator PlaySprite(int spriteIndex)
        {
            CurrentFrame = _currentAnimationElement.GetFrameOfSprite(spriteIndex);
            return this;
        }

        public FrameAnimator PlayFrame(int frameNum)
        {
            CurrentFrame = frameNum;
            return this;
        }

        public FrameAnimator Replay()
        {
            _currentAnimationElement.Reset();
            return this;
        }

        public FrameAnimator Run()
        {
            _isRunning = true;
            _currentAnimationElement.Reset();
            return this;
        }

        public FrameAnimator Stop()
        {
            _isRunning = false;
            _currentAnimationElement.Reset();
            return this;
        }

        public FrameAnimator Pause()
        {
            _isRunning = false;
            return this;
        }

        public FrameAnimator Unpause()
        {
            _isRunning = true;
            return this;
        }

        public FrameAnimator Delay(int delayFrameNum)
        {
            _delayFrame = delayFrameNum;
            return this;
        }
        #endregion

        #region Frame Callback
        [SerializeField]
        private StateMachine _sm;

        private Dictionary<int, Action> _frameEventDict = new Dictionary<int, Action>();
        
        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
            _sm.Init(_animations.Select((e) => e.Name));
            _sm.OnUpdate += FrameEventHandler;
        }

        public void AddEnterEvent(string animationName, Action action) => _sm.AddEnterEvent(animationName, action);
        public void RemoveEnterEvent(string animationName, Action action) => _sm.RemoveEnterEvent(animationName, action);
        public void AddStayEvent(string animationName, Action action) => _sm.AddStayEvent(animationName, action);
        public void RemoveStayEvent(string animationName, Action action) => _sm.RemoveStayEvent(animationName, action);
        public void AddExitEvent(string animationName, Action action) => _sm.AddExitEvent(animationName, action);
        public void RemoveExitEvent(string animationName, Action action) => _sm.RemoveExitEvent(animationName, action);        
        public void AddFrameEvent(string animationName, int frame, Action action)
        {
            int key = GetFrameAnimationKey(animationName, frame);

            _frameEventDict.TryAdd(key, delegate { });
            
            _frameEventDict[key] += action;
        }

        public void RemoveFrameEvent(string animationName, int frame, Action action)
        {
            int key = GetFrameAnimationKey(animationName, frame);
            
            if (_frameEventDict.TryGetValue(key, out Action handler))
            {
                handler -= action;
                if (handler.GetInvocationList().Length == 0)
                {
                    _frameEventDict.Remove(key);
                }
            }
        }

        private void FrameEventHandler()
        {
            if (_animationIsCompletedOnThisFrame)
            {
                for (int i = _previousFrame+1; i <= _animations[_previousAnimationIndex].TotalFrameNum; i++)
                {
                    if (_frameEventDict.TryGetValue(GetFrameAnimationKey(_animations[_previousAnimationIndex].Name, i), out Action action))
                    {
                        action?.Invoke();
                    }
                }

                for (int i = 0; i < CurrentFrame; i++)
                {
                    if (_frameEventDict.TryGetValue(GetFrameAnimationKey(CurrentAnimationName, i), out Action action))
                    {
                        action?.Invoke();
                    }
                }
            }
            else
            {
                for (int i = _previousFrame+1; i <= CurrentFrame; i++)
                {
                    if (_frameEventDict.TryGetValue(GetFrameAnimationKey(CurrentAnimationName, i), out Action action))
                    {
                        action?.Invoke();
                    }
                }
            }
        }

        private int GetFrameAnimationKey(string animationName, int frame)
        {
            if (tryFindAnimationElementIndex(animationName, out int animationIndex))
            {
                return animationIndex * 10000 + frame;
            }

            return -1;
        }

        #endregion
        
        #region Managing Data
        
        public int TotalFrameSinceInit { get; private set; }

        [SerializeField]
        [HideInInspector]
        private int _currentAnimationIndex;
        private AnimationElement _currentAnimationElement => _animations[_currentAnimationIndex];
        public string CurrentAnimationName => _currentAnimationElement.Name;
        public int CurrentFrame
        {
            get => _currentAnimationElement.CurrentFrame;
            private set => _currentAnimationElement.CurrentFrame = value;
        }

        public ReadOnlySpan<Rect> CurrentBodyBoxes => _currentAnimationElement.GetBodyBoxes(CurrentFrame);
        public ReadOnlySpan<Rect> CurrentHitBoxes => _currentAnimationElement.GetHitBoxes(CurrentFrame);
        public ReadOnlySpan<Rect> CurrentAttackBoxes => _currentAnimationElement.GetAttackBoxes(CurrentFrame);
        
        public string[] GetAnimationNames()
        {
            string[] names = new string[_animations.Count];

            for (int i = 0; i < _animations.Count; i++)
            {
                names[i] = _animations[i].Name;
            }
                    
            return names;
        }
        
        public string GetAnimationName(int elementIndex) => _animations[elementIndex].Name;

        public ReadOnlyCollection<Sprite> GetSprites(string animationName)
        {
            if (tryFindAnimationElementIndex(animationName, out int index))
            {
                return new ReadOnlyCollection<Sprite>(_animations[index].Sprites);
            }

            return null;
        }

        private bool tryFindAnimationElementIndex(string animationName, out int elementIndex)
        {
            elementIndex = -1;
            
            for (int i = 0; i < _animations.Count; i++)
            {
                if (animationName.Equals(_animations[i].Name, StringComparison.OrdinalIgnoreCase))
                {
                    elementIndex = i;
                    break;
                }
            }

            if (elementIndex >= 0)
            {
                return true;
            }
            else
            {
                Debug.LogError($"{animationName}이름의 애니메이션을 찾을 수 없습니다.");
                return false;
            }
        }
        #endregion

        #region Private functions
        private bool IsValidAnimationIndex(int index)
        {
            return (index >= 0 && index < _animations.Count);
        }
        #endregion
    }
}