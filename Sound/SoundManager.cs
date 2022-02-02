using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace Utility
{
    [CreateAssetMenu(fileName = "SoundArgs", menuName = "ScriptableObjects/SoundArgs")]
    [Serializable] public class SoundArgs : ScriptableObject
    {
        [SerializeField][HideInInspector] private string EnumName;
        [SerializeField][HideInInspector] private string EnumValue;
        [HideInInspector] public bool EnumNameCorrect;
        public Enum SoundNaming
        {
            get
            {
                Enum val;
                try
                {
                    Type enumType = EnumHelper.GetEnumType(EnumName);
                    if (!Enum.IsDefined(enumType, EnumValue))
                        EnumValue = ((Enum)Enum.GetValues(enumType).GetValue(0)).ToString();
                    val = Enum.Parse(enumType, EnumValue) as Enum;
                } catch
                {
                    // Debug.LogWarning($"ObjectPool({gameObject.name}) EnumName or EnumValue is wrong. Check again.");
                    return null;
                }
                return val;
            }
        }
        public SoundManager.SoundPlayMode SoundPlayMode;
        public Transform Transform;
        public Vector3 RelativePosition;
        public bool AutoReturn;

        public SoundArgs(string EnumValue)
        {
            EnumName = EnumValue;
        }

        public SoundArgs(Type EnumType)
        {
            EnumName = EnumType.AssemblyQualifiedName;
        }
    }

    [Serializable]
    public class SoundManager : Singleton<SoundManager>
    {
        // For Editor Script
        [SerializeField] private bool EnumNameCorrect;
        [SerializeField] private string _enumTypeName;
        [SerializeField] private string _enumString;
        [SerializeField] private bool ShowSettingsInEditor;
        [SerializeField] private bool ShowPathSceneInEditor;
        [SerializeField] private bool ShowDatasInEditor;
        [SerializeField] private bool ShowPreloadedClipsInEditor;
        [SerializeField] private bool ShowSharedClipsInEditor;
        [SerializeField] private bool ShowCurrentClipsInEditor;
        [SerializeField] private bool ShowPlayingSourcesInEditor;
        [SerializeField] private int NamingInterval = 100;
        [SerializeField] private int SharingNamingRegion = 0;
        [SerializeField] private string SharingSoundsPath = string.Empty;
        [SerializeField] private string ResourcePathPrefix = "Sounds/";
        [SerializeField] private int WorldAudioSourceCount = 5;
        [SerializeField] private AudioClip BackGroundMusicClip;
        //

        public enum SoundPlayMode
        {
            OnWorld, OnTransform, At,
        }

        [Serializable] public struct PathScene
        {
            public string Path;
            public string Scene;

            public PathScene (string path, string scene)
            {
                Path = path;
                Scene = scene;
            }
        }

        public Enum EnumValue
        {
            get
            {
                Type type = EnumHelper.GetEnumType(EnumTypeName);
                // Debug.LogFormat("EnumTypeName : {0}, _enumString : {1}, EnumType : {2}", EnumTypeName, _enumString, type.ToString());
                return Enum.Parse(type, _enumString) as Enum;
            }
            set
            {
                _enumString = value.ToString();
            }
        }

        public List<AudioClip> PreloadedAudioClipList = new List<AudioClip>();
        public Dictionary<Enum, AudioClip> SharedAudioClipDict = new Dictionary<Enum, AudioClip>();
        public Dictionary<Enum, AudioClip> CurrentAudioClipDict = new Dictionary<Enum, AudioClip>();
        public Dictionary<Enum, AudioSource> PlayingAudioSourceDict = new Dictionary<Enum, AudioSource>();
        private Queue<AudioSource> _audioSourcesQueue = new Queue<AudioSource>();

        public string EnumTypeName {get{return _enumTypeName;}}

        public bool UsePathSceneSync;
        public List<PathScene> ResourcePathsForEachScene = new List<PathScene>();
        public AudioSource BackGroundAudioSource;

        private int _currentRegion = 0;
        private int _namingStart
        {
            get {return _currentRegion * NamingInterval;}
        }
        private int _namingEnd
        {
            get {return _namingStart + NamingInterval;}
        }

        void Awake()
        {
            // BackGroundMusic Start
            BackGroundAudioSource = gameObject.AddComponent<AudioSource>();
            BackGroundAudioSource.clip = BackGroundMusicClip;
            BackGroundAudioSource.playOnAwake = true;
            BackGroundAudioSource.loop = true;
            BackGroundAudioSource.Play();

            // SceneLoad CallBack Insert
            SceneManager.sceneLoaded += OnSceneLoad;

            // Add audiosources
            for (int i = 0; i < WorldAudioSourceCount; i++)
            {
                var v = gameObject.AddComponent<AudioSource>();
                // fold added component
                _audioSourcesQueue.Enqueue(v);
            }
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(BackGroundAudioSource, false);
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
                try
                {
                    enumValue = Enum.Parse(EnumValue.GetType(), clip.name) as Enum;
                } catch
                {
                    Debug.LogWarning("Shared Sounds in folder and EnumName are not match");
                }
                SharedAudioClipDict.Add(enumValue, clip);
            }
        }

        public virtual void LoadSounds(int targetRegion)
        {
            _currentRegion = targetRegion;

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

        private void PlayOnWorld(Enum soundName)
        {
            AudioSource audioSource = _audioSourcesQueue.Dequeue();

            if (audioSource == null)
            {
                return;
            }

            audioSource.clip = GetClip(soundName);
            audioSource.spatialBlend = 0;
            audioSource.Play();
            _audioSourcesQueue.Enqueue(audioSource);
        }
        

        private void PlayAt(Enum soundName, Vector3 absolutePos)
        {
            AudioClip clip = GetClip(soundName);

            if (clip == null)
            {
                Debug.Log(soundName + " is not loaded");
                return;
            }

            AudioSource.PlayClipAtPoint(clip, absolutePos);
        }

        private void PlayAt(Enum soundName, Transform obj, Vector3 relativePos = new Vector3())
        {
            PlayAt(soundName, obj.position + relativePos);
        }

        public void SetBackGroundMusic(AudioClip clip)
        {
            BackGroundAudioSource.clip = clip;
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
            if (!UsePathSceneSync) return;

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
