using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utility
{
    [Serializable]
    public class SoundManager : Singleton<SoundManager>
    {
        // For Editor Script
        public string EnumName;
        public string EnumString;
        public bool EnumNameCorrect;
        public bool ShowSettingsInEditor;
        public bool ShowPathSceneInEditor;
        public bool ShowDatasInEditor;
        public bool ShowPreloadedClipsInEditor;
        public bool ShowSharedClipsInEditor;
        public bool ShowCurrentClipsInEditor;
        public bool ShowPlayingSourcesInEditor;
        //

        public enum SoundPlayMode
        {
            OnWorld, OnTransform, At,
        }

        [Serializable] public class SoundArgs
        {
            public Enum SoundNaming;
            public SoundPlayMode SoundPlayMode;
            public Transform Transform;
            public Vector3 RelativePosition;
            public bool AutoReturn;
        }

        public Enum EnumValue
        {
            get
            {
                Type type = EnumHelper.GetEnumType(EnumName);
                // Debug.LogFormat("EnumName : {0}, EnumString : {1}, EnumType : {2}", EnumName, EnumString, type.ToString());
                return Enum.Parse(type, EnumString) as Enum;
            }
            set
            {
                EnumString = value.ToString();
            }
        }

        public List<AudioClip> PreloadedAudioClipList = new List<AudioClip>();
        public Dictionary<Enum, AudioClip> SharedAudioClipDict = new Dictionary<Enum, AudioClip>();
        public Dictionary<Enum, AudioClip> CurrentAudioClipDict = new Dictionary<Enum, AudioClip>();
        public Dictionary<Enum, AudioSource> PlayingAudioSourceDict = new Dictionary<Enum, AudioSource>();


        public int NamingInterval = 100;
        public int SharingNamingRegion = 0;
        public string SharingSoundsPath = string.Empty;

        public string ResourcePathPrefix = "Sounds/";
        public bool UsePathSceneSync;
        public List<(string Path, string Scene)> ResourcePathsForEachScene = new List<(string Path, string Scene)>();

        public int WorldAudioSourceCount = 5;
        private Queue<AudioSource> audioSourcesQueue = new Queue<AudioSource>();

        public AudioClip BackGroundMusicClip;
        private AudioSource _backGroundAudioSource;

        private int CurrentRegion = 0;
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
            // BackGroundMusic Start
            _backGroundAudioSource = gameObject.AddComponent<AudioSource>();
            _backGroundAudioSource.clip = BackGroundMusicClip;
            _backGroundAudioSource.playOnAwake = true;
            _backGroundAudioSource.loop = true;
            _backGroundAudioSource.Play();

            // SceneLoad CallBack Insert
            SceneManager.sceneLoaded += OnSceneLoad;

            // Add audiosources
            for (int i = 0; i < WorldAudioSourceCount; i++)
            {
                var v = gameObject.AddComponent<AudioSource>();
                // fold added component
                audioSourcesQueue.Enqueue(v);
            }
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(_backGroundAudioSource, false);
        }

        void Start()
        {
            LoadSharedSounds();
        }

        protected virtual void LoadSharedSounds()
        {
            string finalPath = Path.Combine(ResourcePathPrefix, SharingSoundsPath);

            AudioClip[] clips = Resources.LoadAll<AudioClip>(finalPath);
            foreach(AudioClip clip in clips)
            {
                Enum enumValue = null;
                enumValue = Enum.Parse(EnumValue.GetType(), clip.name) as Enum;
                SharedAudioClipDict.Add(enumValue, clip);
            }
        }

        public virtual void LoadSounds(int targetRegion)
        {
            CurrentRegion = targetRegion;

            int indexOfEnumStart, indexOfEnumEnd;

            EnumHelper.ClapIndexOfEnum<Enum>(_namingStart, _namingEnd, out indexOfEnumStart, out indexOfEnumEnd);

            Enum[] namings = Enum.GetValues(EnumValue.GetType()) as Enum[];

            for (int i = indexOfEnumStart; i<=indexOfEnumEnd; i++)
            {
                AudioClip clip = ResourcesExtension.LoadSubDirectory<AudioClip>(Path.Combine(ResourcePathPrefix + namings[i].ToString())) as AudioClip;
                CurrentAudioClipDict.Add(namings[i], clip);
            }
        }

        public virtual void LoadSounds(string path)
        {
            string finalPath = Path.Combine(ResourcePathPrefix, path);

            AudioClip clip = Resources.Load<AudioClip>(finalPath);
            Enum enumValue = Enum.Parse(EnumValue.GetType(), clip.name) as Enum;
            CurrentAudioClipDict.Add(enumValue, clip);
        }

        public virtual void LoadSoundsInFolder(string path)
        {
            string finalPath = Path.Combine(ResourcePathPrefix, path);

            AudioClip[] clips = Resources.LoadAll<AudioClip>(finalPath);
            foreach(AudioClip clip in clips)
            {
                Enum enumValue = Enum.Parse(EnumValue.GetType(), clip.name) as Enum;
                CurrentAudioClipDict.Add(enumValue, clip);
            }
        }

        protected virtual void ClearCurrentSounds()
        {
            foreach (var playingAudio in PlayingAudioSourceDict)
            {
                playingAudio.Value.Stop();
            }

            CurrentAudioClipDict.Clear();
            PlayingAudioSourceDict.Clear();
        }

        /// <summary>
        /// Play Sound with SoundArgs
        /// </summary>
        /// <param name="args"></param>
        public void Play(SoundArgs args)
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
        /// Play sound at relative position
        /// </summary>
        /// <param name="soundName">The Audio Clip to play</param>
        /// <param name="obj">The Transform on where play clip</param>
        /// <param name="relativePos"></param>
        /// <param name="Return"></param>
        private void PlayOnTransform(Enum soundName, Transform obj, Vector3 relativePos = new Vector3(), bool autoReturn = false)
        {
            AudioSource source;
            GameObject audioObj;

            if (PlayingAudioSourceDict.TryGetValue(soundName, out source))
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

            PlayingAudioSourceDict.Add(soundName, source);

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
            PlayAt(soundName, obj.position + relativePos);
        }

        public void Pause(Enum soundName)
        {
            AudioSource source;
            if (PlayingAudioSourceDict.TryGetValue(soundName, out source))
                source.Pause();
        }

        public void UnPause(Enum soundName)
        {
            AudioSource source;
            if (PlayingAudioSourceDict.TryGetValue(soundName, out source))
                source.UnPause();
        }

        public void PauseAll()
        {
            foreach (KeyValuePair<Enum, AudioSource> source in PlayingAudioSourceDict)
            {
                source.Value.Pause();
            }
        }

        public void UnPauseAll()
        {
            foreach (KeyValuePair<Enum, AudioSource> source in PlayingAudioSourceDict)
            {
                source.Value.UnPause();
            }
        }

        public void Mute(Enum soundName)
        {
            AudioSource source;
            if (PlayingAudioSourceDict.TryGetValue(soundName, out source))
                source.mute = true;
        }

        public void UnMute(Enum soundName)
        {
            AudioSource source;
            if (PlayingAudioSourceDict.TryGetValue(soundName, out source))
                source.mute = false;
        }

        public void MuteAll()
        {
            foreach (KeyValuePair<Enum, AudioSource> source in PlayingAudioSourceDict)
            {
                source.Value.mute = true;
            }
        }

        public void UnMuteAll()
        {
            foreach (KeyValuePair<Enum, AudioSource> source in PlayingAudioSourceDict)
            {
                source.Value.mute = false;
            }
        }

        private IEnumerator ReturnSound(AudioSource source, Enum soundName, Transform obj)
        {
            yield return new WaitWhile(() => source.isPlaying);

            PlayingAudioSourceDict.Remove(soundName);
            Destroy(source.gameObject);
        }

        private AudioClip GetClip(Enum soundName)
        {
            AudioClip clip;
            if (CurrentAudioClipDict.TryGetValue(soundName, out clip)) {}
            else if (SharedAudioClipDict.TryGetValue(soundName, out clip)) {}
            else if (clip = PreloadedAudioClipList.Find(x => x.name == soundName.ToString())) {}
            else 
            {
                Debug.Log(soundName + " is not loaded");
                return null;
            }
            return clip;
        }

        void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            for (int i = 0; i < ResourcePathsForEachScene.Count; i++)
            {
                if (ResourcePathsForEachScene[i].Scene == scene.name)
                {
                    ClearCurrentSounds();
                    LoadSoundsInFolder(ResourcePathsForEachScene[i].Path);
                    return;
                }
            }
        }
    }

}
