﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace dkstlzu.Utility
{
    [RequireComponent(typeof(AudioSource))]
    public class RandomAudioClip : MonoBehaviour
    {
        [Serializable]
        public struct ClipWeight
        {
            public AudioClip Clip;
            public float Weight;

            public static implicit operator ClipWeight(AudioClip clip) => new ClipWeight() { Clip = clip, Weight = 1 };
        }
        
        public List<ClipWeight> Clips;

        [SerializeField]
        private AudioSource _audioSource;

        private void Awake()
        {
            if (!_audioSource)
            {
                _audioSource = GetComponentInChildren<AudioSource>();
            }
        }

        public void Play()
        {
            if (Clips.Count > 0)
            {
                float totalWeight = 0;

                foreach (ClipWeight clipWeight in Clips)
                {
                    totalWeight += clipWeight.Weight;
                }

                float randomWeight = Random.Range(0, totalWeight);
                int index = 0;
                
                for (int i = 0; i < Clips.Count; i++)
                {
                    randomWeight -= Clips[i].Weight;
                    if (randomWeight <= 0)
                    {
                        index = i;
                        break;
                    }
                }
                
                _audioSource.clip = Clips[index].Clip;
            }

            _audioSource.Play();
        }
    }
}