using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Enumerable = System.Linq.Enumerable;

namespace dkstlzu.Utility
{
    public interface IUpdateManager
    {
        public string Name { get; }
        public int Count { get; }

        void ManagerUpdate(float delta);
    }
    
    public interface IUpdatableBase{}
    public interface IUpdatableBase<T> : IUpdatableBase
    {
        public delegate void Updater(IUpdatableBase<T> updatable);
    }
    
    public abstract class UpdateManager<T, TUpdatable> : IUpdateManager where TUpdatable : IUpdatableBase<T>
    {
        protected class HashComparer : IComparer<TUpdatable>
        {
            public int Compare(TUpdatable x, TUpdatable y)
            {
                return y.GetHashCode() - x.GetHashCode();
            }
        }

        protected SortedList<int, Dictionary<int, TUpdatable>> _updatableList = new SortedList<int, Dictionary<int, TUpdatable>>();
        protected SortedList<int, Dictionary<int, int>> _updatableCounts = new SortedList<int, Dictionary<int, int>>();
        protected SortedList<int, List<int>> _keyDict = new SortedList<int, List<int>>();
        protected HashSet<int> _orderSet = new HashSet<int>();
        protected int[] _orders;

        public string Name { get; protected set; }
        public int Count
        {
            get
            {
                int count = 0;

                foreach (var pair in _updatableCounts)
                {
                    count += pair.Value.Count;
                }

                return count;
            }
        }

        private string _exceptionMsg;
        protected T _updateDelta;
        protected IUpdatableBase<T>.Updater _updater;

        public UpdateManager(string managerName, IUpdatableBase<T>.Updater updater)
        {
            Name = managerName;
            _updater = updater;
            _exceptionMsg = $"{Name} UpdateManager.Update() 중에 문제가 발생했습니다. 해당 객체를 리스트에서 제외합니다.";
        }

        /// <summary>
        /// 작은 order 값부터 update됩니다
        /// </summary>
        public void Register(TUpdatable updatable, int order = 0)
        {
            if (_orderSet.Add(order))
            {
                _orders = Enumerable.ToArray(_orderSet);
            }
            
            if (!_updatableList.TryGetValue(order, out var updatableDict))
            {
                updatableDict = new Dictionary<int, TUpdatable>();
                _updatableList.Add(order, updatableDict);
            }

            if (!_updatableCounts.TryGetValue(order, out var countDict))
            {
                countDict = new Dictionary<int, int>();
                _updatableCounts.Add(order, countDict);
            }

            if (!_keyDict.TryGetValue(order, out var keyList))
            {
                keyList = new List<int>();
                _keyDict.Add(order, keyList);
            }
            
            int hashCode = updatable.GetHashCode();

            updatableDict.TryAdd(hashCode, updatable);
            if (!countDict.TryAdd(hashCode, 1))
            {
                countDict[hashCode]++;
            }
            keyList.Add(hashCode);
        }

        public void Unregister(TUpdatable updatable, int order = 0)
        {
            int hashCode = updatable.GetHashCode();
            
            if (_updatableCounts.TryGetValue(order, out var countDict) && countDict.ContainsKey(hashCode))
            {
                countDict[hashCode]--;
                if (countDict[hashCode] <= 0)
                {
                    countDict.Remove(hashCode);
                    if (_updatableList.TryGetValue(order, out var updatableList))
                    {
                        updatableList.Remove(hashCode);
                    }

                    if (_keyDict.TryGetValue(order, out var keyList))
                    {
                        keyList.Remove(hashCode);
                    }
                }

                if (countDict.Count == 0)
                {
                    _orderSet.Remove(order);
                    _orders = Enumerable.ToArray(_orderSet);
                }
            }
        }

        public void Clear()
        {
            foreach (var pair in _updatableList)
            {
                pair.Value.Clear();
            }
            
            foreach (var pair in _updatableCounts)
            {
                pair.Value.Clear();
            }

            _updatableList.Clear();
            _updatableCounts.Clear();
        }

        public void ManagerUpdate(float delta)
        {
            SetDelta(delta);

            UpdateElements();
        }

        private void UpdateElements()
        {
            for (int i = 0; i < _orders.Length; i++)
            {
                int order = _orders[i];

                Dictionary<int, TUpdatable> updatableDict = _updatableList[order];
                Dictionary<int, int> countDict = _updatableCounts[order];
                List<int> hashCodes = _keyDict[order];

                for (int j = 0; j < hashCodes.Count; j++)
                {
                    for (int k = 0; k < countDict[hashCodes[j]]; k++)
                    {
                        _updater.Invoke(updatableDict[hashCodes[j]]);
                    }
                }
            }
        }

        /// <summary>
        /// Must Set _updateDelta here
        /// </summary>
        protected abstract void SetDelta(float delta);
        
        public abstract class System
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            static void RuntimeInit()
            {
                _systemDict.Clear();
            }

            private static SortedDictionary<string, Func<int>> _systemDict = new SortedDictionary<string, Func<int>>();

            private TUpdatable _updater;

            public UpdateManager<T, TUpdatable> Manager { get; protected set; }
            public event Action OnRegister;
            public event Action OnUnregister;

            private string _systemName;
            private int _registeredOrder;

            public Func<int> UpdateOrderGetter
            {
                get => _systemDict[_systemName];
                set => _systemDict[_systemName] = value;
            }

            protected System(string systemName)
            {
                _systemName = systemName;

                if (!_systemDict.ContainsKey(_systemName))
                {
                    _systemDict.Add(_systemName, () => 0);
                }
            }

            public void UpdateAfter(System other) => UpdateAfter(other._systemName);

            public void UpdateAfter(string otherSystemName)
            {
                if (_systemDict.ContainsKey(otherSystemName))
                {
                    _systemDict[_systemName] = () => _systemDict[otherSystemName]() + 1;
                    return;
                }

                _systemDict[_systemName] = () =>
                {
                    if (_systemDict.ContainsKey(otherSystemName))
                    {
                        _systemDict[_systemName] = () => _systemDict[otherSystemName]() + 1;
                        return UpdateOrderGetter();
                    }

                    return 0;
                };
            }

            public void UpdateBefore(System other) => UpdateBefore(other._systemName);

            public void UpdateBefore(string otherSystemName)
            {
                if (_systemDict.ContainsKey(otherSystemName))
                {
                    _systemDict[_systemName] = () => _systemDict[otherSystemName]() - 1;
                    return;
                }

                _systemDict[_systemName] = () =>
                {
                    if (_systemDict.ContainsKey(otherSystemName))
                    {
                        _systemDict[_systemName] = () => _systemDict[otherSystemName]() - 1;
                        return UpdateOrderGetter();
                    }

                    return 0;
                };
            }

            public void Register(UpdateManager<T, TUpdatable> manager)
            {
                Assert.IsTrue(this is TUpdatable, $"{GetType()}을 UpdateManager.System으로 사용하기 위해서는 적절한 {typeof(TUpdatable)}인터페이스를 구현해야 합니다.");

                if (this is TUpdatable updatable)
                {
                    Manager = manager;
                    manager.Register(updatable, UpdateOrderGetter());
                    _registeredOrder = UpdateOrderGetter();
                    OnRegister?.Invoke();
                }
            }

            public void Unregister()
            {
                Assert.IsTrue(this is TUpdatable, $"{GetType()}을 UpdateManager.System으로 사용하기 위해서는 적절한 {typeof(TUpdatable)}인터페이스를 구현해야 합니다.");

                if (this is TUpdatable updatable)
                {
                    Manager.Unregister(updatable);
                    OnUnregister?.Invoke();
                }
            }

            protected void Reregister()
            {
                if (_registeredOrder != UpdateOrderGetter())
                {
                    Unregister();
                    Register(Manager);
                }
            }
        }
    }

    public class TimeUpdateManager<TUpdatable> : UpdateManager<float, TUpdatable> where TUpdatable : IUpdatableBase<float>
    {
        public float TimeMultiplier;
        
        public TimeUpdateManager(string managerName, IUpdatableBase<float>.Updater updater, float timeMultiplier = 1) : base(managerName, updater)
        {
            TimeMultiplier = timeMultiplier;
        }

        protected override void SetDelta(float delta)
        {
            _updateDelta = delta * TimeMultiplier;
        }
    }
    
    public interface IManualUpdatable : IUpdatableBase<float>
    {
        void ManualUpdate(float delta);
    }
    
    public class ManualUpdateManager : TimeUpdateManager<IManualUpdatable>
    {
        public static ManualUpdateManager Instance;

        public ManualUpdateManager() : base("DefaultManual", null)
        {
            Instance = this;
            _updater = updater => ((IManualUpdatable)updater).ManualUpdate(_updateDelta);
        }
    }
    
    public interface IFrameUpdatable : IUpdatableBase<float>
    {
        void FrameUpdate(float delta);
    }
    
    public class DefaultFrameUpdateManager : TimeUpdateManager<IFrameUpdatable>
    {
        public static DefaultFrameUpdateManager Instance;
        
        public DefaultFrameUpdateManager() : base("DefaultFrame", null)
        {
            Instance = this;
            _updater = updater => ((IFrameUpdatable)updater).FrameUpdate(_updateDelta);
        }
    }
    
    public interface IFixedUpdatable : IUpdatableBase<float>
    {
        void ManualFixedUpdate(float delta);
    }
    
    public class DefaultFixedUpdateManager : TimeUpdateManager<IFixedUpdatable>
    {
        public static DefaultFixedUpdateManager Instance;

        public DefaultFixedUpdateManager() : base("DefaultFixed", null)
        {
            Instance = this;
            _updater = updater => ((IFixedUpdatable)updater).ManualFixedUpdate(_updateDelta);
        }
    }

    public interface ILateUpdatable : IUpdatableBase<float>
    {
        void ManualLateUpdate(float delta);
    }
    
    public class DefaultLateUpdateManager : TimeUpdateManager<ILateUpdatable>
    {
        public static DefaultLateUpdateManager Instance;

        public DefaultLateUpdateManager() : base("DefaultLate", null)
        {
            Instance = this;
            _updater = updater => ((ILateUpdatable)updater).ManualLateUpdate(_updateDelta);
        }
    }
    
    public class FrameUpdateManager<TUpdatable> : UpdateManager<int, TUpdatable> where TUpdatable : IUpdatableBase<int>
    {
        public int FrameMultiplier;
        
        public FrameUpdateManager(string managerName, IUpdatableBase<int>.Updater updater, int frameMultiplier = 1) : base(managerName, updater)
        {
            FrameMultiplier = frameMultiplier;
        }

        protected override void SetDelta(float delta)
        {
            _updateDelta = (int)(delta * FrameMultiplier);
        }
    }
    
    public interface IFrameCountUpdatable : IUpdatableBase<int>
    {
        void FrameCountUpdate(int frameCount);
    }
    
    public class DefaultFrameCountUpdateManager : FrameUpdateManager<IFrameCountUpdatable>
    {
        public static DefaultFrameCountUpdateManager Instance;

        public DefaultFrameCountUpdateManager() : base("DefaultCount", null)
        {
            Instance = this;
            _updater = updater => ((IFrameCountUpdatable)updater).FrameCountUpdate(_updateDelta);
        }
    }
}