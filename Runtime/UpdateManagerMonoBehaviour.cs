using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace dkstlzu.Utility
{
    [DefaultExecutionOrder(-100)]
    [AddComponentMenu("UpdateManager")]
    public class UpdateManagerMonoBehaviour : MonoBehaviour
    {
        public Dictionary<UpdateManager.Type, List<IUpdateManager>> ManagerDict = new Dictionary<UpdateManager.Type, List<IUpdateManager>>();
        
        private Dictionary<UpdateManager.Type, List<IUpdateManager>> _addList = new Dictionary<UpdateManager.Type, List<IUpdateManager>>();
        private Dictionary<UpdateManager.Type, List<IUpdateManager>> _removeList = new Dictionary<UpdateManager.Type, List<IUpdateManager>>();

#if UNITY_EDITOR
        public int ManualUpdatableNumber;
        public int FrameUpdatableNumber;
        public int FixedUpdatableNumber;
        public int LateUpdatableNumber;
#endif

        private void Awake()
        {
            InitManager(UpdateManager.Type.MANUAL);
            InitManager(UpdateManager.Type.FRAME);
            InitManager(UpdateManager.Type.FIXED);
            InitManager(UpdateManager.Type.LATE);
            
            ManagerDict[UpdateManager.Type.MANUAL].Add(new ManualUpdateManager());
            ManagerDict[UpdateManager.Type.FRAME].Add(new DefaultFrameUpdateManager());
            ManagerDict[UpdateManager.Type.FIXED].Add(new DefaultFixedUpdateManager());
            ManagerDict[UpdateManager.Type.LATE].Add(new DefaultLateUpdateManager());
        }

        void InitManager(UpdateManager.Type updateType)
        {
            ManagerDict.Add(updateType, new List<IUpdateManager>());
            _addList.Add(updateType, new List<IUpdateManager>());
            _removeList.Add(updateType, new List<IUpdateManager>());
        }

        public void AddManager(UpdateManager.Type updateType, IUpdateManager manager)
        {
            if (!_addList.ContainsKey(updateType))
            {
                InitManager(updateType);
            }
            
            _addList[updateType].Add(manager);
        }

        public void RemoveManager(UpdateManager.Type updateType, IUpdateManager manager)
        {
            Assert.IsTrue(_removeList.ContainsKey(updateType));
            
            _removeList[updateType].Add(manager);
        }

        [CanBeNull]
        public IUpdateManager GetManager(UpdateManager.Type updateType, string managerName)
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
            if (updatableBase is IFrameUpdatable updatable)
            {
                DefaultFrameUpdateManager.Instance.Unregister(updatable);
            }

            if (updatableBase is IFixedUpdatable fixedUpdatable)
            {
                DefaultFixedUpdateManager.Instance.Unregister(fixedUpdatable);
            }
            
            if (updatableBase is ILateUpdatable lateUpdatable)
            {
                DefaultLateUpdateManager.Instance.Unregister(lateUpdatable);
            }
        }

        public void ManualUpdate()
        {
#if UNITY_EDITOR
            SetCounter(out ManualUpdatableNumber, ManagerDict[UpdateManager.Type.MANUAL]);
#endif
            UpdateWithDelta(UpdateManager.Type.MANUAL);
        }

        private void Update()
        {
#if UNITY_EDITOR
            SetCounter(out FrameUpdatableNumber, ManagerDict[UpdateManager.Type.FRAME]);
#endif
            UpdateWithDelta(UpdateManager.Type.FRAME);
        }

        private void FixedUpdate()
        {
#if UNITY_EDITOR
            SetCounter(out FixedUpdatableNumber, ManagerDict[UpdateManager.Type.FIXED]);
#endif
            UpdateWithDelta(UpdateManager.Type.FIXED);
        }
        
        private void LateUpdate()
        {
#if UNITY_EDITOR
            SetCounter(out LateUpdatableNumber, ManagerDict[UpdateManager.Type.LATE]);
#endif
            UpdateWithDelta(UpdateManager.Type.LATE);
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

        void UpdateWithDelta(UpdateManager.Type updateType)
        {
            foreach (var add in _addList[updateType])
            {
                ManagerDict[updateType].Add(add);
            }
            _addList[updateType].Clear();
            
            float delta = Time.deltaTime;
            
            foreach (IUpdateManager manager in ManagerDict[updateType])
            {
                try
                {
                    manager.ManagerUpdate(delta);
                }
                catch (Exception e)
                {
                    Printer.Print($"{updateType} UpdateManager.Update() 중에 문제가 발생했습니다." + "\n" + e, logLevel:LogLevel.Error, customTag:"UpdateManager", priority:1);
                    _removeList[updateType].Add(manager);
                    throw;
                }
            }
            
            foreach (var remove in _removeList[updateType])
            {
                ManagerDict[updateType].Remove(remove);
            }
            _removeList[updateType].Clear();
        }
    }
}