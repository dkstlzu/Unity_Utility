using System.Collections.Generic;
using UnityEngine;

namespace dkstlzu.Utility
{
    [CreateAssetMenu(fileName = "Rects", menuName = "FrameAnimation/Collision Boxes", order = 0)]
    public class RectsScriptable : ScriptableObject
    {
        [field: SerializeField] public DoubleList<Rect> Sequences { get; set; }

        public int Count => Sequences != null ? Sequences.Count : 0;

        public List<Rect> this[int index]
        {
            get => Sequences[index];
            set => Sequences[index] = value;
        }
    }
}