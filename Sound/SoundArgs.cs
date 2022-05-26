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
        [HideInInspector] public Transform Transform;
        [HideInInspector] public Vector3 RelativePosition;
        public bool AutoReturn;
    }
}