using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

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

        protected SortedList<int, SortedDictionary<TUpdatable, int>> _updatableList = new SortedList<int, SortedDictionary<TUpdatable, int>>();

        protected List<(int, TUpdatable)> _addList = new List<(int, TUpdatable)>();
        protected List<(int, TUpdatable)> _removeList = new List<(int, TUpdatable)>();

        public string Name { get; protected set; }
        public int Count
        {
            get
            {
                int count = 0;

                foreach (var pair in _updatableList)
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
            _addList.Add((order, updatable));
        }

        public void Unregister(TUpdatable updatable, int order = 0)
        {
            _removeList.Add((order, updatable));
        }

        public void Clear()
        {
            foreach (var pair in _updatableList)
            {
                pair.Value.Clear();
            }

            _updatableList.Clear();
        }

        public void ManagerUpdate(float delta)
        {
            AddUpdatables();
            
            RemoveUpdatables();

            SetDelta(delta);

            UpdateElements();

            // Remove error list
            RemoveUpdatables();
        }

        private void AddUpdatables()
        {
            foreach (var updatable in _addList)
            {
                if (!_updatableList.TryGetValue(updatable.Item1, out var dict))
                {
                    dict = new(new HashComparer());
                    _updatableList.Add(updatable.Item1, dict);
                }

                if (!dict.TryAdd(updatable.Item2, 1))
                {
                    dict[updatable.Item2]++;
                }
            }

            _addList.Clear();
        }

        private void RemoveUpdatables()
        {
            foreach (var updatable in _removeList)
            {
                if (_updatableList.TryGetValue(updatable.Item1, out var dict))
                {
                    if (dict.ContainsKey(updatable.Item2))
                    {
                        dict[updatable.Item2]--;
                        if (dict[updatable.Item2] <= 0)
                        {
                            dict.Remove(updatable.Item2);
                        }
                    }
                }
            }

            _removeList.Clear();
        }
        
        private void UpdateElements()
        {
            foreach (var orderListPair in _updatableList)
            {
                foreach (var updatable in orderListPair.Value)
                {
                    try
                    {
                        for (int i = 0; i < updatable.Value; i++)
                        {
                            _updater.Invoke(updatable.Key);
                        }
                    }
                    catch (Exception e)
                    {
                        if (UpdateManager.EnableLog)
                        {
                            Printer.Print(_exceptionMsg + "\n" + e, logLevel: LogLevel.Error, customTag: "UpdateManager", priority: 1);
                        }
                        _removeList.Add((orderListPair.Key, updatable.Key));
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
}