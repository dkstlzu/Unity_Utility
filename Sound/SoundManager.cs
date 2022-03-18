using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utility
{
    public class SoundManager : Singleton<SoundManager>
    {
        // For Editor Script
        // [SerializeField] private bool EnumNameCorrect;
        // [SerializeField] private string _enumTypeName;
        // [SerializeField] private string _enumString;
        // [SerializeField] private bool ShowSettingsInEditor;
        // [SerializeField] private bool ShowPathSceneInEditor;
        // [SerializeField] private bool ShowDatasInEditor;
        // [SerializeField] private bool ShowPreloadedClipsInEditor;
        // [SerializeField] private bool ShowSharedClipsInEditor;
        // [SerializeField] private bool ShowCurrentClipsInEditor;
        // [SerializeField] private bool ShowPlayingSourcesInEditor;
        // [SerializeField] private int NamingInterval = 100;
        // [SerializeField] private int SharingNamingRegion = 0;
        // [SerializeField] private string SharingSoundsPath = string.Empty;
        // [SerializeField] private string ResourcePathPrefix = "Sounds/";
        [SerializeField] private int WorldAudioSourceCount = 5;
        [SerializeField] private AudioClip BackGroundMusicClip;
        //

        // [Serializable] public struct PathScene
        // {
        //     public string Path;
        //     public string Scene;

        //     public PathScene (string path, string scene)
        //     {
        //         Path = path;
        //         Scene = scene;
        //     }
        // }

        // public Enum EnumValue
        // {
        //     get
        //     {
        //         Type type = EnumHelper.GetEnumType(_enumTypeName);
        //         // Debug.LogFormat("EnumTypeName : {0}, _enumString : {1}, EnumType : {2}", EnumTypeName, _enumString, type.ToString());
        //         return Enum.Parse(type, _enumString) as Enum;
        //     }
        //     set
        //     {
        //         _enumString = value.ToString();
        //     }
        // }

        // public List<AudioClip> PreloadedAudioClipList = new List<AudioClip>();
        // public Dictionary<Enum, AudioClip> SharedAudioClipDict = new Dictionary<Enum, AudioClip>();
        // public Dictionary<Enum, AudioClip> CurrentAudioClipDict = new Dictionary<Enum, AudioClip>();
        public Dictionary<AudioClip, AudioSource> PlayingAudioSourceDict = new Dictionary<AudioClip, AudioSource>();
        private Queue<AudioSource> _audioSourcesQueue = new Queue<AudioSource>();

        // public bool UsePathSceneSync;
        // public List<PathScene> ResourcePathsForEachScene = new List<PathScene>();
        public AudioSource BackGroundAudioSource;

        // private int _currentRegion = 0;
        // private int _namingStart
        // {
        //     get {return _currentRegion * NamingInterval;}
        // }
        // private int _namingEnd
        // {
        //     get {return _namingStart + NamingInterval;}
        // }

        void Awake()
        {
            // BackGroundMusic Start
            BackGroundAudioSource = gameObject.AddComponent<AudioSource>();
            BackGroundAudioSource.clip = BackGroundMusicClip;
            BackGroundAudioSource.playOnAwake = true;
            BackGroundAudioSource.loop = true;
            BackGroundAudioSource.Play();

            // SceneLoad CallBack Insert
            // SceneManager.sceneLoaded += OnSceneLoad;

            // Add audiosources
            for (int i = 0; i < WorldAudioSourceCount; i++)
            {
                var v = gameObject.AddComponent<AudioSource>();
                // fold added component
                _audioSourcesQueue.Enqueue(v);
            }
#if UNITY_EDITOR
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(BackGroundAudioSource, false);
#endif
        }

        // void Start()
        // {
        //     LoadSharedSounds();
        // }

        // protected virtual void LoadSharedSounds()
        // {
        //     LoadSounds(SharingNamingRegion);
        // }

        
        // public virtual void LoadSound (AudioClip clip)
        // {
            
        // }

        // public virtual void LoadSounds(int targetRegion)
        // {
        //     print(targetRegion);
        //     _currentRegion = targetRegion;

        //     int indexOfEnumStart, indexOfEnumEnd;

        //     EnumHelper.ClapIndexOfEnum(EnumValue.GetType(), _namingStart, _namingEnd, out indexOfEnumStart, out indexOfEnumEnd);

        //     Enum[] namings = Enum.GetValues(EnumValue.GetType()) as Enum[];

        //     print($"{indexOfEnumStart}, {indexOfEnumEnd}, {_namingStart}, {_namingEnd}");
        //     print(namings.Length);

        //     for (int i = indexOfEnumStart; i<=indexOfEnumEnd; i++)
        //     {
        //         print(namings[i]);
        //         AudioClip clip = ResourcesExtension.LoadSubDirectory<AudioClip>(ResourcePathPrefix, namings[i].ToString()) as AudioClip;
        //         print(clip);
        //         CurrentAudioClipDict.Add(namings[i], clip);
        //     }
        // }

        // public virtual void LoadSound(string path)
        // {
        //     string finalPath = Path.Combine(ResourcePathPrefix, path);

        //     AudioClip clip = Resources.Load<AudioClip>(finalPath);
        //     Enum enumValue = Enum.Parse(EnumValue.GetType(), clip.name) as Enum;
        //     CurrentAudioClipDict.Add(enumValue, clip);
        // }

        // public virtual void LoadSoundsInFolder(string path)
        // {
        //     string finalPath = Path.Combine(ResourcePathPrefix, path);

        //     AudioClip[] clips = Resources.LoadAll<AudioClip>(finalPath);
        //     foreach(AudioClip clip in clips)
        //     {
        //         Enum enumValue = Enum.Parse(EnumValue.GetType(), clip.name) as Enum;
        //         CurrentAudioClipDict.Add(enumValue, clip);
        //     }
        // }

        // protected virtual void ClearCurrentSounds()
        // {
        //     foreach (var playingAudio in PlayingAudioSourceDict)
        //     {
        //         playingAudio.Value.Stop();
        //     }

        //     CurrentAudioClipDict.Clear();
        //     PlayingAudioSourceDict.Clear();
        // }

        /// <summary>
        /// Play Sound with SoundArgs
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

        // private AudioClip GetClip(AudioClip clip)
        // {
        //     AudioClip clip;
        //     if (CurrentAudioClipDict.TryGetValue(soundName, out clip)) {}
        //     else if (SharedAudioClipDict.TryGetValue(soundName, out clip)) {}
        //     else if (clip = PreloadedAudioClipList.Find(x => x.name == soundName.ToString())) {}
        //     else 
        //     {
        //         Debug.Log(soundName + " is not loaded");
        //         return null;
        //     }
        //     return clip;
        // }

        // void OnSceneLoad(Scene scene, LoadSceneMode mode)
        // {
        //     if (!UsePathSceneSync) return;

        //     for (int i = 0; i < ResourcePathsForEachScene.Count; i++)
        //     {
        //         if (ResourcePathsForEachScene[i].Scene == scene.name)
        //         {
        //             ClearCurrentSounds();
        //             LoadSoundsInFolder(ResourcePathsForEachScene[i].Path);
        //             return;
        //         }
        //     }
        // }
    }
}
