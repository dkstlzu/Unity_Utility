using UnityEngine;

namespace dkstlzu.Utility
{
    [CreateAssetMenu(fileName = "AnimationSprites", menuName = "FrameAnimation/AnimationSprites", order = 0)]
    public class FrameAnimationSprites : ScriptableObject
    {
        [field: SerializeField] public Sprite[] Sequences { get; set; }
    }
}