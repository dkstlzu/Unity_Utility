using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using Utility.Naming;

namespace Utility
{
    [Serializable]
    public class SoundManager : Singleton<SoundManager>
    {
        [Serializable] public class DictionaryOfEnumAudioClip : SerializableDictionary<Enum, AudioClip> {}
        [Serializable] public class DictionaryOfEnumAudioSource : SerializableDictionary<Enum, AudioSource> {}
        public string EnumTypeName;
        public Type enumType;
        public enum SoundPlayMode
        {
            OnWorld, OnTransform, At,
        }

        [Serializable] public class SoundEventArgs
        {
            public SoundPlayMode SoundPlayMode;
            public Enum NamingType;
            public Transform Transform;
            public Vector3 RelativePosition;
            public bool AutoReturn;
        }

        [Serializable] class SoundEvent : UnityEvent<SoundEventArgs>
        {
            public static SoundEvent soundEvent = new SoundEvent();
        }

        [Serializable] public class PathAndSceneTuple : StringTuple
        {
            public string Path
            {
                get {return String1;}
                set {String1 = value;}
            }

            public string Scene
            {
                get {return String2;}
                set {String2 = value;}
            }
        }
        
        [SerializeField] protected DictionaryOfEnumAudioClip sharedAudioClipDict = new DictionaryOfEnumAudioClip();
        [SerializeField] protected DictionaryOfEnumAudioClip currentAudioClipDict = new DictionaryOfEnumAudioClip();
        [SerializeField] protected DictionaryOfEnumAudioSource playingAudioSourceDict = new DictionaryOfEnumAudioSource();


        public int NamingInterval = 100;
        public int SharingNamingRegion = 0;
        [SerializeField] protected string resourcePathPrefix = "Sounds/";
        [SerializeField] protected List<PathAndSceneTuple> resourcePathsForEachScene = new List<PathAndSceneTuple>();

        public int WorldAudioSourceCount = 5;
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
            // Initializing enumType
            enumType = Type.GetType(EnumTypeName);

            // Initializing Collections
            for (int i = 0; i < WorldAudioSourceCount; i++)
            {
                audioSourcesQueue.Enqueue(gameObject.AddComponent<AudioSource>());
            }

            // SceneLoad CallBack Insert
            SceneManager.sceneLoaded += OnSceneLoad;

            // BackGroundMusic Start
            _backGroundAudioSource = gameObject.AddComponent<AudioSource>();
            _backGroundAudioSource.clip = BackGroundMusicClip;
            _backGroundAudioSource.playOnAwake = true;
            _backGroundAudioSource.loop = true;
            _backGroundAudioSource.Play();

            SoundEvent.soundEvent.AddListener(Play);
        }

        void Start()
        {
            LoadSharedSounds();
        }

        protected virtual void LoadSharedSounds()
        {
            CurrentRegion = SharingNamingRegion;
            string sharedResourcePath;
            if (resourcePathsForEachScene.Count == 0)
            {
                print("SoundManager ResourcePathsForEachScene is empty. Check your Component.");
                return;
            }
            else
                sharedResourcePath = resourcePathPrefix + resourcePathsForEachScene[SharingNamingRegion].Path;

            int indexOfEnumStart = 0;
            int indexOfEnumEnd = 0;

            var genericEnumMethod = typeof(EnumHelper).GetMethod("ClapIndexOfEnum").MakeGenericMethod(enumType);
            object[] enumMethodParms = new object[]{_namingStart, _namingEnd, indexOfEnumStart, indexOfEnumEnd};
            genericEnumMethod.Invoke(null, enumMethodParms);
            indexOfEnumStart = (int)enumMethodParms[2];
            indexOfEnumEnd = (int)enumMethodParms[3];
            // EnumHelper.ClapIndexOfEnum<>(_namingStart, _namingEnd, out indexOfEnumStart, out indexOfEnumEnd);

            print($"Loading SharedSounds from naming {_namingStart} to {_namingEnd} whose enum from {indexOfEnumStart} to {indexOfEnumEnd}");

            Array namings = enumType.GetEnumValues();

            dynamic enumVal;
            for (int i = indexOfEnumStart; i <= indexOfEnumEnd; i++)
            {
                enumVal = namings.GetValue(i);
                AudioClip clip = Resources.Load<AudioClip>(sharedResourcePath + enumVal.ToString());
                sharedAudioClipDict.Add(enumVal, clip);
            }
        }

        protected virtual void LoadSounds(int targetRegion)
        {
            ClearCurrentSounds();

            string stageSoundResourcePath = resourcePathPrefix;

            CurrentRegion = targetRegion;
            stageSoundResourcePath += resourcePathsForEachScene[CurrentRegion].Path;

            int indexOfEnumStart;
            int indexOfEnumEnd;

            EnumHelper.ClapIndexOfEnum<Enum>(_namingStart, _namingEnd, out indexOfEnumStart, out indexOfEnumEnd);

            Enum[] namings = Enum.GetValues(typeof(Enum)) as Enum[];

            for (int i = indexOfEnumStart; i<=indexOfEnumEnd; i++)
            {
                AudioClip clip = Resources.Load<AudioClip>(stageSoundResourcePath + namings[i].ToString());
                currentAudioClipDict.Add(namings[i], clip);
            }
        }

        protected virtual void ClearCurrentSounds()
        {
            foreach (var playingAudio in playingAudioSourceDict)
            {
                playingAudio.Value.Stop();
            }

            currentAudioClipDict.Clear();
            playingAudioSourceDict.Clear();
        }

        public void Play(SoundEventArgs args)
        {
            switch(args.SoundPlayMode)
            {
                case SoundPlayMode.At :
                if (args.Transform == null)
                    PlayAt(args.NamingType, args.RelativePosition);
                else
                    PlayAt(args.NamingType, args.Transform, args.RelativePosition);
                break;
                case SoundPlayMode.OnTransform :
                    PlayOnTransform(args.NamingType, args.Transform, args.RelativePosition, args.AutoReturn);
                break;
                case SoundPlayMode.OnWorld :
                    PlayOnWorld(args.NamingType);
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
        private void Play(Enum soundName, Transform obj, Vector3 relativePos = new Vector3(), bool autoReturn = false)
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

        public void PlayOnWorld(Enum soundName)
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
        
        public void PlayOnTransform(Enum soundName, Transform obj, Vector3 relativePos = new Vector3(), bool autoReturn = false)
        {
            Play(soundName, obj.transform, relativePos, autoReturn: autoReturn);
        }

        public void PlayAt(Enum soundName, Vector3 absolutePos)
        {
            AudioClip clip = GetClip(soundName);

            if (clip == null)
            {
                Debug.Log(soundName + " is not loaded");
                return;
            }

            AudioSource.PlayClipAtPoint(clip, absolutePos);
        }

        public void PlayAt(Enum soundName, Transform obj, Vector3 relativePos = new Vector3())
        {
            AudioClip clip = GetClip(soundName);

            if (clip == null)
            {
                Debug.Log(soundName + " is not loaded");
                return;
            }

            AudioSource.PlayClipAtPoint(clip, obj.position + relativePos);
        }

        public void Pause(Enum soundName)
        {
            AudioSource source;
            if (playingAudioSourceDict.TryGetValue(soundName, out source))
                source.Pause();
        }

        public void UnPause(Enum soundName)
        {
            AudioSource source;
            if (playingAudioSourceDict.TryGetValue(soundName, out source))
                source.UnPause();
        }

        public void PauseAll()
        {
            foreach (KeyValuePair<Enum, AudioSource> source in playingAudioSourceDict)
            {
                source.Value.Pause();
            }
        }

        public void UnPauseAll()
        {
            foreach (KeyValuePair<Enum, AudioSource> source in playingAudioSourceDict)
            {
                source.Value.UnPause();
            }
        }

        public void Mute(Enum soundName)
        {
            AudioSource source;
            if (playingAudioSourceDict.TryGetValue(soundName, out source))
                source.mute = true;
        }

        public void UnMute(Enum soundName)
        {
            AudioSource source;
            if (playingAudioSourceDict.TryGetValue(soundName, out source))
                source.mute = false;
        }

        public void MuteAll()
        {
            foreach (KeyValuePair<Enum, AudioSource> source in playingAudioSourceDict)
            {
                source.Value.mute = true;
            }
        }

        public void UnMuteAll()
        {
            foreach (KeyValuePair<Enum, AudioSource> source in playingAudioSourceDict)
            {
                source.Value.mute = false;
            }
        }

        private IEnumerator ReturnSound(AudioSource source, Enum soundName, Transform obj)
        {
            yield return new WaitWhile(() => source.isPlaying);

            playingAudioSourceDict.Remove(soundName);
            Destroy(source.gameObject);
        }

        private AudioClip GetClip(Enum soundName)
        {
            AudioClip clip;
            if (currentAudioClipDict.TryGetValue(soundName, out clip)) {}
            else if (sharedAudioClipDict.TryGetValue(soundName, out clip)) {}
            else 
            {
                Debug.Log(soundName + " is not loaded");
                return null;
            }
            return clip;
        }

        void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            for (int i = 0; i < resourcePathsForEachScene.Count; i++)
            {
                if (resourcePathsForEachScene[i].Scene == scene.name)
                {
                    LoadSounds(i);
                    return;
                }
            }
        }
    }

}
