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

    public class SoundManager : Singleton<SoundManager>
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
                BackGroundAudioSource.Play();
            }
        }

        public bool Use3DSoundSetting;
        public bool IsPaused;
        public bool IsMuted;

        public List<AudioSource> PlayingAudioSourceList;

        [Serializable]
        public class ClipInfo
        {
            public string Name;
            public AudioClip Clip;
        }

        // Odin을 기본으로 사용한다는 가정을 하기 어려워 간단한 형태로 대체합니다.
        public List<ClipInfo> PreloadedClipList;
        public Dictionary<string, AudioClip> ClipDict = new Dictionary<string, AudioClip>();
        public Dictionary<string, AudioSource> AudioSourceDict = new Dictionary<string, AudioSource>();

        void Awake()
        {
            BackGroundAudioSourceSetting();
            SetPreloadedClips();
            _audioSourcePool.Init();
        }

        void BackGroundAudioSourceSetting()
        {
            if (BackGroundAudioSource == null)
            {
                BackGroundAudioSource = gameObject.AddComponent<AudioSource>();
            }
            
            BackGroundAudioSource.clip = backGroundMusicClip;
            BackGroundAudioSource.loop = true;
            BackGroundAudioSource.Play();
            
            // Fold Audiosource Components
#if UNITY_EDITOR
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(BackGroundAudioSource, false);
#endif
        }

        void SetPreloadedClips()
        {
            foreach (ClipInfo clipInfo in PreloadedClipList)
            {
                if (clipInfo == null || clipInfo.Name == string.Empty || clipInfo.Clip == null)
                {
                    continue;
                }
                
                ClipDict.Add(clipInfo.Name, clipInfo.Clip);
            }
        }

        public void Play(string clipName) => Play(clipName, 1, false);
        public void Play(string clipName, bool loop) => Play(clipName, 1, loop);
        public void Play(string clipName, float volume) => Play(clipName, volume, false);
        public void Play(string clipName, float volume, bool loop)
        {
            if (!ClipDict.ContainsKey(clipName))
            {
                return;
            }
            
            PlayOnWorld(ClipDict[clipName], volume, loop, true);
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
                    PlayOnWorld(clip, volume, args.LoopOnWorld, args.AutoReturn);
                break;
            }
        }

        public void PlayAt(AudioClip clip, Vector3 position, bool loop)
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
        public void PlayOnTransform(AudioClip clip, Transform obj, Vector3 relativePos, bool autoDestroy)
        {
            GameObject audioObj = new GameObject("AudioObject");
            audioObj.transform.SetParent(obj);
            audioObj.transform.localPosition = relativePos;
            
            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = Use3DSoundSetting ? 1 : 0;
            source.Play();

            PlayingAudioSourceList.Add(source);

            if (autoDestroy)
            {
                StartCoroutine(DestroySound(source));
            }
        }

        public void PlayOnWorld(AudioClip clip, float volume, bool loop, bool autoReturn)
        {
            AudioSource source = _audioSourcePool.Get();

            if (source == null)
            {
                Printer.Print("All World Audio Source is being used");
                return;
            }
            
            source.clip = clip;
            source.volume = volume;
            source.spatialBlend = 0;
            source.loop = loop;
            source.Play();
            
            if (IsPaused) source.Pause();
            source.mute = IsMuted;
            
            PlayingAudioSourceList.Add(source);

            if (autoReturn)
            {
                StartCoroutine(ReturnSound(source));
            }
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

        private IEnumerator DestroySound(AudioSource source)
        {
            yield return new WaitWhile(() => source.isPlaying);
            
            PlayingAudioSourceList.Remove(source);
            Destroy(source.gameObject);
        }
        
        private IEnumerator ReturnSound(AudioSource source)
        {
            yield return new WaitWhile(() => source.isPlaying);

            PlayingAudioSourceList.Remove(source);
            _audioSourcePool.Return(source);
        }
    }
}
