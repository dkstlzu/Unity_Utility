using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

using Utility.Naming;

namespace Utility
{
    [System.Serializable]
    public class DictionaryOfAudioClip : SerializableDictionary<SoundNaming, AudioClip> { }    
    [System.Serializable]
    public class DictionaryOfAudioSource : SerializableDictionary<SoundNaming, AudioSource> { }    
    public enum SoundPlayMode
    {
        OnWorld, OnTransform, At,
    }
    public class SoundManager : Singleton<SoundManager>
    {
        [SerializeField] private DictionaryOfAudioClip SharedAudioClipDict;
        [SerializeField] private DictionaryOfAudioClip CurrentAudioClipDict;
        [SerializeField] private DictionaryOfAudioSource playingAudioSourceDict;

        public int NamingInterval = 100;
        public int SharingNamingRegion = 0;
        [SerializeField] private string resourcePathPrefix = "Sounds/";
        [SerializeField] private List<String> resourcePathsForEachStage = new List<string>();

        public int WorldAudioSourceNumber = 5;
        private Queue<AudioSource> audioSourcesQueue = new Queue<AudioSource>();

        public AudioClip BackGroundMusicClip;
        private AudioSource _backGroundAudioSource;

        public int CurrentRegion = 0;
        private int _namingStart
        {
            get {return CurrentRegion * NamingInterval;}
        }
        private int _namingEnd
        {
            get {return _namingStart + NamingInterval;}
        }

        void Awake()
        {
            SharedAudioClipDict = new DictionaryOfAudioClip();
            CurrentAudioClipDict = new DictionaryOfAudioClip();
            playingAudioSourceDict = new DictionaryOfAudioSource();

            for (int i = 0; i < WorldAudioSourceNumber; i++)
            {
                audioSourcesQueue.Enqueue(gameObject.AddComponent<AudioSource>());
            }

            _backGroundAudioSource = gameObject.AddComponent<AudioSource>();
            _backGroundAudioSource.clip = BackGroundMusicClip;
            _backGroundAudioSource.playOnAwake = true;
            _backGroundAudioSource.loop = true;
            _backGroundAudioSource.Play();

            SoundEvent.soundEvent.AddListener(Play);
        }

        void Start()
        {
            LoadSharedSounds(SharingNamingRegion);
        }

        protected virtual void LoadSharedSounds(int targetRegion)
        {
            CurrentRegion = targetRegion;
            string sharedResourcePath = resourcePathPrefix + resourcePathsForEachStage[targetRegion];

            int indexOfEnumStart;
            int indexOfEnumEnd;

            EnumHelper.ClapIndexOfEnum<SoundNaming>(_namingStart, _namingEnd, out indexOfEnumStart, out indexOfEnumEnd);

            print($"{_namingStart} {_namingEnd} {indexOfEnumStart} {indexOfEnumEnd}");

            SoundNaming[] namings = Enum.GetValues(typeof(SoundNaming)) as SoundNaming[];

            for (int i = indexOfEnumStart; i <= indexOfEnumEnd; i++)
            {
                AudioClip clip = Resources.Load<AudioClip>(sharedResourcePath + namings[i].ToString());
                SharedAudioClipDict.Add(namings[i], clip);
            }
        }

        protected virtual void LoadSounds(int targetRegion)
        {
            ClearCurrentSounds();

            string stageSoundResourcePath = resourcePathPrefix;

            CurrentRegion = targetRegion;
            stageSoundResourcePath += resourcePathsForEachStage[CurrentRegion];

            int indexOfEnumStart;
            int indexOfEnumEnd;

            EnumHelper.ClapIndexOfEnum<SoundNaming>(_namingStart, _namingEnd, out indexOfEnumStart, out indexOfEnumEnd);

            SoundNaming[] namings = Enum.GetValues(typeof(SoundNaming)) as SoundNaming[];

            for (int i = indexOfEnumStart; i<=indexOfEnumEnd; i++)
            {
                AudioClip clip = Resources.Load<AudioClip>(stageSoundResourcePath + namings[i].ToString());
                CurrentAudioClipDict.Add(namings[i], clip);
            }
        }

        protected virtual void ClearCurrentSounds()
        {
            foreach (var playingAudio in playingAudioSourceDict)
            {
                playingAudio.Value.Stop();
            }

            CurrentAudioClipDict.Clear();
            playingAudioSourceDict.Clear();
        }

        public void Play(SoundEventArgs args)
        {
            switch(args.SoundPlayMode)
            {
                case SoundPlayMode.At :
                if (args.Transform == null)
                    PlayAt(args.SoundNaming, args.RelativePosition);
                else
                    PlayAt(args.SoundNaming, args.Transform, args.RelativePosition);
                break;
                case SoundPlayMode.OnTransform :
                    PlayOnTransform(args.SoundNaming, args.Transform, args.RelativePosition, args.AutoReturn);
                break;
                case SoundPlayMode.OnWorld :
                    PlayOnWorld(args.SoundNaming);
                break;
            }
        }

        /// <summary>
        /// Final Play Method
        /// </summary>
        /// <param name="soundName">The Audio Clip to play</param>
        /// <param name="obj">The Transform on where play clip</param>
        /// <param name="relativePos"></param>
        /// <param name="Return"></param>
        private void Play(SoundNaming soundName, Transform obj, Vector3 relativePos = new Vector3(), bool autoReturn = false)
        {
            AudioSource source;
            GameObject audioObj;

            if (playingAudioSourceDict.TryGetValue(soundName, out source))
            { 
                if (!source.isPlaying) source.Play(); 
                return;
            }

            audioObj = new GameObject("AudioObject");
            source = audioObj.AddComponent<AudioSource>();

            source.clip = GetClip(soundName);

            if (source.clip == null)
            {
                Debug.Log(soundName + " is not loaded");
                Destroy(audioObj);
                Destroy(source);
                return;
            }

            audioObj.transform.SetParent(obj);
            audioObj.transform.localPosition = relativePos;

            playingAudioSourceDict.Add(soundName, source);

            // Set spatialBlen as 0 if you want 2D sound
            // Set spatialBlen as 1 if you want 3D sound
            source.spatialBlend = 1;
            source.Play();

            if (autoReturn)
            {
                StartCoroutine(ReturnSound(source, soundName, obj));
            }
        }

        public void PlayOnWorld(SoundNaming soundName)
        {
            AudioSource audioSource = audioSourcesQueue.Dequeue();

            if (audioSource == null)
            {
                return;
            }

            audioSource.clip = GetClip(soundName);
            audioSource.spatialBlend = 0;
            audioSource.Play();
            audioSourcesQueue.Enqueue(audioSource);
        }
        
        public void PlayOnTransform(SoundNaming soundName, Transform obj, Vector3 relativePos = new Vector3(), bool autoReturn = false)
        {
            Play(soundName, obj.transform, relativePos, autoReturn: autoReturn);
        }

        public void PlayAt(SoundNaming soundName, Vector3 absolutePos)
        {
            AudioClip clip = GetClip(soundName);

            if (clip == null)
            {
                Debug.Log(soundName + " is not loaded");
                return;
            }

            AudioSource.PlayClipAtPoint(clip, absolutePos);
        }

        public void PlayAt(SoundNaming soundName, Transform obj, Vector3 relativePos = new Vector3())
        {
            AudioClip clip = GetClip(soundName);

            if (clip == null)
            {
                Debug.Log(soundName + " is not loaded");
                return;
            }

            AudioSource.PlayClipAtPoint(clip, obj.position + relativePos);
        }

        public void Pause(SoundNaming soundName)
        {
            AudioSource source;
            if (playingAudioSourceDict.TryGetValue(soundName, out source))
                source.Pause();
        }

        public void UnPause(SoundNaming soundName)
        {
            AudioSource source;
            if (playingAudioSourceDict.TryGetValue(soundName, out source))
                source.UnPause();
        }

        public void PauseAll()
        {
            foreach (KeyValuePair<SoundNaming, AudioSource> source in playingAudioSourceDict)
            {
                source.Value.Pause();
            }
        }

        public void UnPauseAll()
        {
            foreach (KeyValuePair<SoundNaming, AudioSource> source in playingAudioSourceDict)
            {
                source.Value.UnPause();
            }
        }

        public void Mute(SoundNaming soundName)
        {
            AudioSource source;
            if (playingAudioSourceDict.TryGetValue(soundName, out source))
                source.mute = true;
        }

        public void UnMute(SoundNaming soundName)
        {
            AudioSource source;
            if (playingAudioSourceDict.TryGetValue(soundName, out source))
                source.mute = false;
        }

        public void MuteAll()
        {
            foreach (KeyValuePair<SoundNaming, AudioSource> source in playingAudioSourceDict)
            {
                source.Value.mute = true;
            }
        }

        public void UnMuteAll()
        {
            foreach (KeyValuePair<SoundNaming, AudioSource> source in playingAudioSourceDict)
            {
                source.Value.mute = false;
            }
        }

        private IEnumerator ReturnSound(AudioSource source, SoundNaming soundName, Transform obj)
        {
            yield return new WaitWhile(() => source.isPlaying);

            playingAudioSourceDict.Remove(soundName);
            Destroy(source.gameObject);
        }

        private AudioClip GetClip(SoundNaming soundName)
        {
            AudioClip clip;
            if (CurrentAudioClipDict.TryGetValue(soundName, out clip)) {}
            else if (SharedAudioClipDict.TryGetValue(soundName, out clip)) {}
            else 
            {
                Debug.Log(soundName + " is not loaded");
                return null;
            }
            return clip;
        }
    }

    [System.Serializable]
    public class SoundEventArgs
    {
        public SoundPlayMode SoundPlayMode;
        public SoundNaming SoundNaming;
        public Transform Transform;
        public Vector3 RelativePosition;
        public bool AutoReturn;
    }

    [System.Serializable] public class SoundEvent : UnityEvent<SoundEventArgs>
    {
        public static SoundEvent soundEvent = new SoundEvent();
    }
}
