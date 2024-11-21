using System;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace dkstlzu.Utility
{
    [CreateAssetMenu(fileName = "FrameAnimation", menuName = "FrameAnimation/FrameAnimation", order = 0)]
    public class FrameAnimationInfoScriptable : ScriptableObject
    {
        [field: SerializeField, SerializeReference] public FrameAnimationSprites Sprites { get; set; }
        [field: SerializeField, SerializeReference] public FrameAnimationFrames Frames{ get; set; }
        [field: SerializeField, SerializeReference] public RectsScriptable BodyBoxRects{ get; set; }
        [field: SerializeField, SerializeReference] public RectsScriptable HitBoxRects{ get; set; }
        [field: SerializeField, SerializeReference] public RectsScriptable AttackBoxRects{ get; set; }

        public int TotalSpriteNum => Sprites != null ? Sprites.Sequences.Length : 0;
        public int TotalFrameDataNum => Frames != null ? Frames.Sequences.Length : 0;
        public int TotalFrameNum => Frames != null ? Frames.GetTotalFrameNum() : 0;

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                return;
            }
#endif
            Assert.IsTrue(IsValid(), $"프레임 애니메이션의 프레임 수와 이미지 수가 다릅니다. {this}");
        }

        public bool IsValid()
        {
            if (!SpriteFrameIsValid() || !BoxesAreValid()) return false;

            bool result = TotalFrameDataNum.Equals(TotalSpriteNum);
            return result;
        }

        private bool SpriteFrameIsValid()
        {
            if (!Sprites || !Frames || Sprites.Sequences == null || Frames.Sequences == null)
            {
                return false;
            }

            return true;
        }

        private bool BoxesAreValid()
        {
            if ((BodyBoxRects != null && !BodyBoxRects.Count.Equals(TotalFrameNum))
                || (HitBoxRects != null && !HitBoxRects.Count.Equals(TotalFrameNum))
                || (AttackBoxRects != null && !AttackBoxRects.Count.Equals(TotalFrameNum)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get Sprite in Sequence by sprite index
        /// </summary>
        /// <param name="spriteIndex">Greater or Equal than 0</param>
        public Sprite GetSpriteOfIndex(int spriteIndex)
        {
            if (!SpriteFrameIsValid() || spriteIndex < 0 || spriteIndex >= TotalSpriteNum)
            {
                return null;
            }
            
            return Sprites.Sequences[spriteIndex];
        }

        /// <summary>
        /// Get Sprite in Sequence by frame
        /// </summary>
        /// <param name="frameNum">Greater or Equal than 0, Less or Equal than TotalFrameNum</param>
        /// <returns>null if input is out of bound</returns>
        public Sprite GetSpriteOfFrame(int frameNum)
        {
            if (!SpriteFrameIsValid() || frameNum < 0 || frameNum > TotalFrameNum)
            {
                return null;
            }

            return GetSpriteOfIndex(GetSpriteIndexOfFrame(frameNum));
        }

        /// <summary>
        /// Get SpriteIndex in Sequence by frame 
        /// </summary>
        /// <param name="frameNum">Greater or Equal than 0, Less or Equal than TotalFrameNum</param>
        /// <returns>null if input is out of bound</returns>
        public int GetSpriteIndexOfFrame(int frameNum)
        {
            if (!SpriteFrameIsValid() || frameNum < 0 || frameNum > TotalFrameNum)
            {
                return -1;
            }
            
            for (int i = 0; i < Frames.Sequences.Length; i++)
            {
                if (frameNum <= Frames.Sequences[i])
                {
                    return i;
                }

                frameNum -= Frames.Sequences[i];
            }

            return -1;
        }

        /// <summary>
        /// Get Frame in Sequence by spriteIndex
        /// </summary>
        /// <param name="spriteIndex">Greater or Equal than 0</param>
        /// <returns>-1 if input is out of bound</returns>
        public int GetFrameOfSpriteIndex(int spriteIndex)
        {
            if (!SpriteFrameIsValid() || spriteIndex < 0 || spriteIndex >= TotalSpriteNum)
            {
                return -1;
            }

            // 해당 sprite의 시작 frame을 가리키기 위해 1을 더함
            int result = 1;

            // frame 수를 해당 sprite 직전까지 축적
            for (int i = 0; i < spriteIndex; i++)
            {
                result += Frames.Sequences[i];
            }

            return result;
        }

        /// <summary>
        /// Get Body Boxes of target frame
        /// </summary>
        public ReadOnlySpan<Rect> GetBodyRectsOfFrame(int frame)
        {
            if (frame <= 0 || frame > TotalFrameNum) return new ReadOnlySpan<Rect>(Array.Empty<Rect>());
            
            if (BodyBoxRects != null && BodyBoxRects.Count >= frame)
            {
                return new ReadOnlySpan<Rect>(BodyBoxRects[frame-1].ToArray());
            }
            else
            {
                return new ReadOnlySpan<Rect>(Array.Empty<Rect>());
            }
        }
        
        /// <summary>
        /// Get Hit Boxes of target frame
        /// </summary>
        public ReadOnlySpan<Rect> GetHitRectsOfFrame(int frame)
        {
            if (frame <= 0 || frame > TotalFrameNum) return new ReadOnlySpan<Rect>(Array.Empty<Rect>());

            if (HitBoxRects != null && HitBoxRects.Count >= frame)
            {
                return new ReadOnlySpan<Rect>(HitBoxRects[frame-1].ToArray());
            }
            else
            {
                return new ReadOnlySpan<Rect>(Array.Empty<Rect>());
            }
        }
        
        /// <summary>
        /// Get Attack Boxes of target frame
        /// </summary>
        public ReadOnlySpan<Rect> GetAttackRectsOfFrame(int frame)
        {
            if (frame <= 0 || frame > TotalFrameNum) return new ReadOnlySpan<Rect>(Array.Empty<Rect>());

            if (AttackBoxRects != null && AttackBoxRects.Count >= frame)
            {
                return new ReadOnlySpan<Rect>(AttackBoxRects[frame-1].ToArray());
            }
            else
            {
                return new ReadOnlySpan<Rect>(Array.Empty<Rect>());
            }
        }
    }
}