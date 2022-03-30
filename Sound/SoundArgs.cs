using System;
using UnityEngine;

namespace Utility
{
    [CreateAssetMenu(fileName = "SoundArgs", menuName = "ScriptableObjects/SoundArgs")]
    [Serializable] public class SoundArgs : ScriptableObject
    {
        public enum SoundPlayMode
        {
            OnWorld, OnTransform, At,
        }

        public SoundPlayMode PlayMode;
        public Transform Transform;
        public Vector3 RelativePosition;
        public bool AutoReturn;
    }
}