using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace dkstlzu.Utility
{
    [Serializable]
    public class RandomAudioClip
    {
        public AudioClip[] Clips;

        public AudioClip Get()
        {
            if (Clips == null || Clips.Length == 0)
            {
                return null;
            }

            return Clips[Random.Range(0, Clips.Length)];
        }
    }

    public class SoundManager : Singleton<SoundManager>
    {
        [SerializeField]
        private BehaviourObjectPool<AudioSource> _audioSourcePool;
        [SerializeField]
        private GameObjectPool<AudioSource> _movableAudioSourcePool;

        public AnimationCurve AudioSourceSpatialCurve;
        
        [Serializable]
        public class BGMInfo
        {
#if UNITY_EDITOR
            public SceneAsset Scene;
#endif
            public string SceneName;
            public AudioClip BGMClip;
        }
        
        public AudioSource BackGroundAudioSource;
        public List<BGMInfo> BGMInfoList;

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
            _audioSourcePool.Init((audioSource) =>
            {
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                audioSource.spatialBlend = 0;
            });

            _movableAudioSourcePool.Init((audioSource) =>
            {
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                audioSource.spatialBlend = 1;
                audioSource.rolloffMode = AudioRolloffMode.Custom;
                audioSource.minDistance = 10;
                audioSource.maxDistance = 100;
                audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, AudioSourceSpatialCurve);
            });
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= BGMSetOnSceneLoaded;
        }

        void BackGroundAudioSourceSetting()
        {
            if (BackGroundAudioSource == null)
            {
                BackGroundAudioSource = gameObject.AddComponent<AudioSource>();
            }
            
            BackGroundAudioSource.loop = true;
            
            SceneManager.sceneLoaded += BGMSetOnSceneLoaded;
        }

        private void BGMSetOnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            foreach (var bgmInfo in BGMInfoList)
            {
                if (bgmInfo.SceneName == scene.name)
                {
                    BackGroundAudioSource.clip = bgmInfo.BGMClip;
                    BackGroundAudioSource.Play();
                    return;
                }
            }
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

        /// <summary>
        /// Play Preloaded Clip with name
        /// </summary>
        public void Play(string clipName) => Play(clipName, 1, false);
        /// <summary>
        /// Play Preloaded Clip with name
        /// </summary>
        public void Play(string clipName, bool loop) => Play(clipName, 1, loop);
        /// <summary>
        /// Play Preloaded Clip with name
        /// </summary>
        public void Play(string clipName, float volume) => Play(clipName, volume, false);
        /// <summary>
        /// Play Preloaded Clip with name
        /// </summary>
        public void Play(string clipName, float volume, bool loop)
        {
            if (!ClipDict.ContainsKey(clipName))
            {
                return;
            }
            
            PlayOnWorld(ClipDict[clipName], volume, loop);
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

        public void PlayAt(AudioClip clip, Vector3 position, bool loop = false)
        {
            AudioSource source = _movableAudioSourcePool.Get();

            if (source == null)
            {
                Printer.Print("All Movable Audio Source is being used");
                return;
            }
            
            source.transform.position = position;
            source.clip = clip;
            source.loop = loop;
            source.Play();
            
            PlayingAudioSourceList.Add(source);

            StartCoroutine(ReturnMovableSound(source));
            
            if (!loop)
            {
                StartCoroutine(ReturnMovableSound(source));
            }
        }

        /// <summary>
        /// Play sound at relative position
        /// </summary>
        public void PlayOnTransform(AudioClip clip, Transform on, bool loop = false, Vector3 relativePos = default)
        {
            AudioSource source = _movableAudioSourcePool.Get();
            
            if (source == null)
            {
                Printer.Print("All Movable Audio Source is being used");
                return;
            }
            
            source.transform.SetParent(on);
            source.transform.localPosition = relativePos;
            source.loop = loop;
            source.clip = clip;
            source.Play();

            PlayingAudioSourceList.Add(source);

            if (!loop)
            {
                StartCoroutine(ReturnMovableSound(source));
            }
        }

        public void PlayOnWorld(AudioClip clip, float volume = 1, bool loop = false)
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

            if (!loop)
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
            yield return new WaitForSeconds(source.clip.length);
            
            PlayingAudioSourceList.Remove(source);
            Destroy(source.gameObject);
        }
        
        private IEnumerator ReturnSound(AudioSource source)
        {
            yield return new WaitForSeconds(source.clip.length);

            PlayingAudioSourceList.Remove(source);
            _audioSourcePool.Return(source);
        }
        
        private IEnumerator ReturnMovableSound(AudioSource source)
        {
            yield return new WaitForSeconds(source.clip.length);

            if (source != null)
            {
                source.transform.SetParent(transform);
                _movableAudioSourcePool.Return(source);
            }
            
            PlayingAudioSourceList.Remove(source);
        }
    }
}
