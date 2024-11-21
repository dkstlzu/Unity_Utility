using UnityEngine;

namespace dkstlzu.Utility
{
    [CreateAssetMenu(fileName = "AnimationFrames", menuName = "FrameAnimation/AnimationFrames", order = 0)]
    public class FrameAnimationFrames : ScriptableObject
    {
        [field: SerializeField] public int[] Sequences{ get; set; }
        
        public int GetTotalFrameNum()
        {
            int num = 0;
            
            foreach (var frame in Sequences)
            {
                num += frame;
            }

            return num;
        }
    }
}