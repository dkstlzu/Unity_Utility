using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utility
{
    [Serializable]
    public class ClipArg
    {
        public AudioClip Clip;
        public SoundArgs Arg;
    }

    public class SoundManager : Singleton<SoundManager>
    {
        [SerializeField] private int WorldAudioSourceCount = 5;
        [SerializeField] private AudioClip backGroundMusicClip;
        public AudioClip BackGroundMusicClip
        {
            get{return backGroundMusicClip;}
            set
            {
                backGroundMusicClip = value;
                BackGroundAudioSource.clip = value;
            }
        }

        public Dictionary<AudioClip, AudioSource> PlayingAudioSourceDict = new Dictionary<AudioClip, AudioSource>();
        private Queue<AudioSource> _audioSourcesQueue = new Queue<AudioSource>();

        public AudioSource BackGroundAudioSource;

        void Awake()
        {
            BackGroundAudioSourceSetting();
            WorldAudioSourceSetting();

            // Fold Audiosource Components
#if UNITY_EDITOR
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(BackGroundAudioSource, false);
#endif
        }

        void BackGroundAudioSourceSetting()
        {
            Transform ChildTransform = new GameObject("BackGround AudioSource").transform;
            ChildTransform.SetParent(transform);
            BackGroundAudioSource = ChildTransform.gameObject.AddComponent<AudioSource>();
            BackGroundAudioSource.clip = backGroundMusicClip;
            BackGroundAudioSource.playOnAwake = true;
            BackGroundAudioSource.loop = true;
            BackGroundAudioSource.Play();
        }
        void WorldAudioSourceSetting()
        {
            // Add audiosources
            for (int i = 0; i < WorldAudioSourceCount; i++)
            {
                var v = gameObject.AddComponent<AudioSource>();
                // fold added component
                _audioSourcesQueue.Enqueue(v);
            }
        }

        /// <summary>
        /// Play Clip with SoundArgs
        /// </summary>
        public void Play(AudioClip clip, SoundArgs args)
        {
            switch(args.PlayMode)
            {
                case SoundArgs.SoundPlayMode.At :
                if (args.Transform == null)
                    PlayAt(clip, args.RelativePosition);
                else
                    PlayAt(clip, args.Transform, args.RelativePosition);
                break;
                case SoundArgs.SoundPlayMode.OnTransform :
                    PlayOnTransform(clip, args.Transform, args.RelativePosition, args.AutoReturn);
                break;
                case SoundArgs.SoundPlayMode.OnWorld :
                    PlayOnWorld(clip);
                break;
            }
        }

        /// <summary>
        /// Play sound at relative position
        /// </summary>
        /// <param name="soundName">The Audio Clip to play</param>
        /// <param name="obj">The Transform on where play clip</param>
        private void PlayOnTransform(AudioClip clip, Transform obj, Vector3 relativePos = new Vector3(), bool autoReturn = false)
        {
            AudioSource source;
            GameObject audioObj;

            if (PlayingAudioSourceDict.TryGetValue(clip, out source))
            { 
                if (!source.isPlaying) source.Play(); 
                return;
            }

            audioObj = new GameObject("AudioObject");
            source = audioObj.AddComponent<AudioSource>();

            source.clip = clip;

            if (source.clip == null)
            {
                Debug.Log(clip + " is not loaded");
                Destroy(audioObj);
                Destroy(source);
                return;
            }

            audioObj.transform.SetParent(obj);
            audioObj.transform.localPosition = relativePos;

            PlayingAudioSourceDict.Add(clip, source);

            // Set spatialBlen as 0 if you want 2D sound
            // Set spatialBlen as 1 if you want 3D sound
            source.spatialBlend = 1;
            source.Play();

            if (autoReturn)
            {
                StartCoroutine(ReturnSound(source, clip, obj));
            }
        }

        private void PlayOnWorld(AudioClip clip)
        {
            AudioSource audioSource = _audioSourcesQueue.Dequeue();

            if (audioSource == null)
            {
                return;
            }

            audioSource.clip = clip;
            audioSource.spatialBlend = 0;
            audioSource.Play();
            _audioSourcesQueue.Enqueue(audioSource);
        }
        

        private void PlayAt(AudioClip clip, Vector3 absolutePos)
        {
            if (clip == null)
            {
                Debug.Log(clip + " is not loaded");
                return;
            }

            AudioSource.PlayClipAtPoint(clip, absolutePos);
        }

        private void PlayAt(AudioClip clip, Transform obj, Vector3 relativePos = new Vector3())
        {
            PlayAt(clip, obj.position + relativePos);
        }

        public void SetBackGroundMusic(AudioClip clip)
        {
            BackGroundAudioSource.clip = clip;
        }

        public void Pause(AudioClip clip)
        {
            AudioSource source;
            if (PlayingAudioSourceDict.TryGetValue(clip, out source))
                source.Pause();
        }

        public void UnPause(AudioClip clip)
        {
            AudioSource source;
            if (PlayingAudioSourceDict.TryGetValue(clip, out source))
                source.UnPause();
        }

        public void PauseAll()
        {
            foreach (KeyValuePair<AudioClip, AudioSource> source in PlayingAudioSourceDict)
            {
                source.Value.Pause();
            }
        }

        public void UnPauseAll()
        {
            foreach (KeyValuePair<AudioClip, AudioSource> source in PlayingAudioSourceDict)
            {
                source.Value.UnPause();
            }
        }

        public void Mute(AudioClip clip)
        {
            AudioSource source;
            if (PlayingAudioSourceDict.TryGetValue(clip, out source))
                source.mute = true;
        }

        public void UnMute(AudioClip clip)
        {
            AudioSource source;
            if (PlayingAudioSourceDict.TryGetValue(clip, out source))
                source.mute = false;
        }

        public void MuteAll()
        {
            foreach (KeyValuePair<AudioClip, AudioSource> source in PlayingAudioSourceDict)
            {
                source.Value.mute = true;
            }
        }

        public void UnMuteAll()
        {
            foreach (KeyValuePair<AudioClip, AudioSource> source in PlayingAudioSourceDict)
            {
                source.Value.mute = false;
            }
        }

        private IEnumerator ReturnSound(AudioSource source, AudioClip clip, Transform obj)
        {
            yield return new WaitWhile(() => source.isPlaying);

            PlayingAudioSourceDict.Remove(clip);
            Destroy(source.gameObject);
        }
    }
}
