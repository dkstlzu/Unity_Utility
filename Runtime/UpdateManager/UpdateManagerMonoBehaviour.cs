using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace dkstlzu.Utility
{
    [DefaultExecutionOrder(-100)]
    [AddComponentMenu("UpdateManager")]
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class UpdateManager : Singleton<UpdateManager>
    {
        public enum Type
        {
            MANUAL,
            FRAME,
            FIXED,
            LATE,
        }

        public static bool EnableLog;

#if UNITY_EDITOR
        private const string UPDATEMANAGER_ENABLELOG_PREFKEY = "UpdateManagerEnableLogPrefKey";
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void LoadEnableLog()
        {
            EnableLog = EditorPrefs.GetBool(UPDATEMANAGER_ENABLELOG_PREFKEY, false);
        }
        
        [MenuItem("Dev/UpdateManager/Enable Log")]
        public static void UpdateLogEnable()
        {
            EnableLog = true;
            EditorPrefs.SetBool(UPDATEMANAGER_ENABLELOG_PREFKEY, EnableLog);
        }
        
        [MenuItem("Dev/UpdateManager/Enable Log", true)]
        static bool LogEnable_Validation()
        {
            return !EnableLog;
        }
        
        [MenuItem("Dev/UpdateManager/Disable Log")]
        public static void UpdateLogDisable()
        {
            EnableLog = false;
            EditorPrefs.SetBool(UPDATEMANAGER_ENABLELOG_PREFKEY, EnableLog);
        }
        
        [MenuItem("Dev/UpdateManager/Disable Log", true)]
        static bool LogDisable_Validation()
        {
            return EnableLog;
        }
#endif
        
        public Dictionary<Type, List<IUpdateManager>> ManagerDict = new Dictionary<Type, List<IUpdateManager>>();
        
        private Dictionary<Type, List<IUpdateManager>> _addList = new Dictionary<Type, List<IUpdateManager>>();
        private Dictionary<Type, List<IUpdateManager>> _removeList = new Dictionary<Type, List<IUpdateManager>>();
        private int _typeCount;

#if UNITY_EDITOR
        public int ManualUpdatableNumber;
        public int FrameUpdatableNumber;
        public int FixedUpdatableNumber;
        public int LateUpdatableNumber;
        public bool _EnableLog;
#endif

        protected virtual void Awake()
        {
            _typeCount = Enum.GetNames(typeof(Type)).Length;
            
            InitManager(Type.MANUAL);
            InitManager(Type.FRAME);
            InitManager(Type.FIXED);
            InitManager(Type.LATE);
            
            ManagerDict[Type.MANUAL].Add(new ManualUpdateManager());
            ManagerDict[Type.FRAME].Add(new DefaultFrameUpdateManager());
            ManagerDict[Type.FIXED].Add(new DefaultFixedUpdateManager());
            ManagerDict[Type.LATE].Add(new DefaultLateUpdateManager());
        }

        void InitManager(Type updateType)
        {
            ManagerDict.Add(updateType, new List<IUpdateManager>());
            _addList.Add(updateType, new List<IUpdateManager>());
            _removeList.Add(updateType, new List<IUpdateManager>());
        }

        public void AddManager(Type updateType, IUpdateManager manager)
        {
            if (!_addList.ContainsKey(updateType))
            {
                InitManager(updateType);
            }
            
            _addList[updateType].Add(manager);
        }

        public void RemoveManager(Type updateType, IUpdateManager manager)
        {
            Assert.IsTrue(_removeList.ContainsKey(updateType));
            
            _removeList[updateType].Add(manager);
        }

        [CanBeNull]
        public IUpdateManager GetManager(Type updateType, string managerName)
        {
            if (!ManagerDict.ContainsKey(updateType))
            {
                return null;
            }

            var manager = ManagerDict[updateType].Find(m => m.Name == managerName);
            
            Assert.IsNotNull(manager);
            return manager;
        }

        public void AddUpdatable(IUpdatableBase updatable)
        {
            bool success = false;
            
            if (updatable is IFrameUpdatable frameUpdatable)
            {
                DefaultFrameUpdateManager.Instance.Register(frameUpdatable);
                success = true;
            }
            
            if (updatable is IFixedUpdatable fixedUpdatable)
            {
                DefaultFixedUpdateManager.Instance.Register(fixedUpdatable);
                success = true;
            }
            
            if (updatable is ILateUpdatable lateUpdatable)
            {
                DefaultLateUpdateManager.Instance.Register(lateUpdatable);
                success = true;
            }
            
            if (updatable is IManualUpdatable manualUpdatable)
            {
                ManualUpdateManager.Instance.Register(manualUpdatable);
                success = true;
            }
            
            if (!success)
            {
                Assert.IsTrue(false, $"updatable은 IUpdatableBase 혹은 IUpdatableBase<T>을 직접 구현해서는 안됩니다.");
            }
        }
        
        public void RemoveUpdatable(IUpdatableBase updatableBase)
        {
            bool success = false;

            if (updatableBase is IFrameUpdatable updatable)
            {
                DefaultFrameUpdateManager.Instance.Unregister(updatable);
                success = true;
            }

            if (updatableBase is IFixedUpdatable fixedUpdatable)
            {
                DefaultFixedUpdateManager.Instance.Unregister(fixedUpdatable);
                success = true;
            }
            
            if (updatableBase is ILateUpdatable lateUpdatable)
            {
                DefaultLateUpdateManager.Instance.Unregister(lateUpdatable);
                success = true;
            }
            
            if (updatableBase is IManualUpdatable manualUpdatable)
            {
                ManualUpdateManager.Instance.Unregister(manualUpdatable);
                success = true;
            }
            
            if (!success)
            {
                Assert.IsTrue(false, $"updatable은 IUpdatableBase 혹은 IUpdatableBase<T>을 직접 구현해서는 안됩니다.");
            }
        }

        public void ManualUpdate()
        {
#if UNITY_EDITOR
            SetCounter(out ManualUpdatableNumber, ManagerDict[Type.MANUAL]);
#endif
            UpdateWithDelta(Type.MANUAL);
        }

        public void Update()
        {
#if UNITY_EDITOR
            _EnableLog = EnableLog;
            SetCounter(out FrameUpdatableNumber, ManagerDict[Type.FRAME]);
#endif
            UpdateWithDelta(Type.FRAME);
        }

        public void FixedUpdate()
        {
#if UNITY_EDITOR
            SetCounter(out FixedUpdatableNumber, ManagerDict[Type.FIXED]);
#endif
            UpdateWithDelta(Type.FIXED);
        }
        
        public void LateUpdate()
        {
#if UNITY_EDITOR
            SetCounter(out LateUpdatableNumber, ManagerDict[Type.LATE]);
#endif
            UpdateWithDelta(Type.LATE);
        }

#if UNITY_EDITOR
        void SetCounter(out int count, IEnumerable<IUpdateManager> managers)
        {
            count = 0;
            
            foreach (IUpdateManager manager in managers)
            {
                count += manager.Count;
            }
        }
#endif

        void UpdateWithDelta(Type updateType)
        {
            var managerList = ManagerDict[updateType];

            for (int i = 0; i < _addList[updateType].Count; i++)
            {
                managerList.Add(_addList[updateType][i]);
            }
            _addList[updateType].Clear();
            
            float delta = Time.deltaTime;

            for (int i = 0; i < managerList.Count; i++)
            {
                try
                {
                    managerList[i].ManagerUpdate(delta);
                }
                catch (Exception e)
                {
                    if (EnableLog)
                    {
                        Printer.Print($"{updateType} {managerList[i].Name} UpdateManager.Update() 중에 문제가 발생했습니다." + "\n" + e, logLevel:LogLevel.Error, customTag:"UpdateManager", priority:1);
                    }
                    _removeList[updateType].Add(managerList[i]);
                }
            }
            
            for (int i = 0; i < _removeList[updateType].Count; i++)
            {
                managerList.Remove(_removeList[updateType][i]);
            }
            _removeList[updateType].Clear();
        }
    }
}