using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace dkstlzu.Utility
{
    [Serializable]
    public class ClipArg
    {
        public AudioClip Clip;
        public SoundArgs Arg;
    }

    public class WorldSoundManager : MonoBehaviour
    {
        [SerializeField]
        private BehaviourObjectPool<AudioSource> _audioSourcePool;

        public AudioSource BackGroundAudioSource;
        [SerializeField] private AudioClip backGroundMusicClip;
        public AudioClip BackGroundMusicClip
        {
            get => backGroundMusicClip;
            set
            {
                backGroundMusicClip = value;
                BackGroundAudioSource.clip = value;
            }
        }

        public bool Use3DSoundSetting;
        public bool IsPaused;
        public bool IsMuted;

        public List<AudioSource> PlayingAudioSourceList = new List<AudioSource>();
        public Dictionary<string, AudioSource> AudioSourceDict = new Dictionary<string, AudioSource>();

        void Awake()
        {
            BackGroundAudioSourceSetting();
            _audioSourcePool.Init();
            
            // Fold Audiosource Components
#if UNITY_EDITOR
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(BackGroundAudioSource, false);
#endif
        }

        void BackGroundAudioSourceSetting()
        {
            if (BackGroundAudioSource == null)
            {
                BackGroundAudioSource = gameObject.AddComponent<AudioSource>();
            }
            
            BackGroundAudioSource.clip = backGroundMusicClip;
            BackGroundAudioSource.Play();
        }

        public void Play(string audioSourceName, AudioClip clip, float volume, bool loop)
        {
            if (!AudioSourceDict.ContainsKey(audioSourceName))
            {
                AddAudioSource(audioSourceName);
            }
            
            AudioSourceDict[audioSourceName].clip = clip;
            AudioSourceDict[audioSourceName].volume = volume;
            AudioSourceDict[audioSourceName].loop = loop;
            AudioSourceDict[audioSourceName].Play();
        }
        
        private void AddAudioSource(string audioSourceName)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.transform.SetParent(transform);
            AudioSourceDict.Add(audioSourceName, source);
        }

        private void RemoveAudioSource(string audioSourceName)
        {
            Destroy(AudioSourceDict[audioSourceName]);
            AudioSourceDict.Remove(audioSourceName);
        }

        /// <summary>
        /// Play Clip with SoundArgs
        /// </summary>
        public void Play(AudioClip clip, SoundArgs args, float volume = 1)
        {
            AudioSource audioSource = _audioSourcePool.Get();

            if (audioSource == null)
            {
                Printer.Print("All World Audio Source is being used");
                return;
            }
                
            switch(args.PlayMode)
            {
                case SoundArgs.SoundPlayMode.At :
                    Vector3 position = args.Transform == null ? args.RelativePosition : args.Transform.position + args.RelativePosition;
                    PlayAt(clip, position, args.LoopOnWorld);
                break;
                case SoundArgs.SoundPlayMode.OnTransform :
                    PlayOnTransform(clip, args.Transform, args.RelativePosition, args.AutoReturn);
                break;
                default:
                case SoundArgs.SoundPlayMode.OnWorld :
                    PlayOnWorld(audioSource, clip, volume, args.LoopOnWorld);
                break;
            }
            
            if (IsPaused) audioSource.Pause();
            audioSource.mute = IsMuted;
        }

        private void PlayAt(AudioClip clip, Vector3 position, bool loop)
        {
            var go = new GameObject("Temporary Audio");
            go.transform.SetParent(transform);
            go.transform.position = position;
            
            var source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = loop;
            source.Play();
        }

        /// <summary>
        /// Play sound at relative position
        /// </summary>
        private void PlayOnTransform(AudioClip clip, Transform obj, Vector3 relativePos, bool autoReturn)
        {
            GameObject audioObj = new GameObject("AudioObject");
            audioObj.transform.SetParent(obj);
            audioObj.transform.localPosition = relativePos;
            
            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = Use3DSoundSetting ? 1 : 0;
            source.Play();

            PlayingAudioSourceList.Add(source);

            if (autoReturn)
            {
                StartCoroutine(ReturnSound(source));
            }
        }

        private void PlayOnWorld(AudioSource source, AudioClip clip, float volume, bool loop)
        {
            source.clip = clip;
            source.volume = volume;
            source.spatialBlend = 0;
            source.loop = loop;
            source.Play();
        }

        public void PauseAll()
        {
            foreach (var source in PlayingAudioSourceList)
            {
                source.Pause();
            }

            IsPaused = true;
        }

        public void UnPauseAll()
        {
            foreach (var source in PlayingAudioSourceList)
            {
                source.UnPause();
            }
            
            IsPaused = false;
        }

        public void MuteAll()
        {
            foreach (var source in PlayingAudioSourceList)
            {
                source.mute = true;
            }

            IsMuted = true;
        }

        public void UnMuteAll()
        {
            foreach (var source in PlayingAudioSourceList)
            {
                source.mute = false;
            }
            
            IsMuted = false;
        }

        private IEnumerator ReturnSound(AudioSource source)
        {
            yield return new WaitWhile(() => source.isPlaying);

            PlayingAudioSourceList.Remove(source);
            Destroy(source.gameObject);
        }
    }
}
