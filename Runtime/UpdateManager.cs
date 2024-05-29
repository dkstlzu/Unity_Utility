using System;
using System.Collections.Generic;
using UnityEngine;

namespace dkstlzu.Utility
{
    [DefaultExecutionOrder(-10)]
    [AddComponentMenu("UpdateManager")]
    public class UpdateManagerMonoBehaviour : Singleton<UpdateManagerMonoBehaviour>
    {
        private UpdateManager _updateManager;
        private FixedUpdateManager _fixedUpdateManager;

#if UNITY_EDITOR
        public int UpdatableNumber;
        public int FixedUpdatableNumber;
#endif

        private void Awake()
        {
            _updateManager = new UpdateManager();
            _fixedUpdateManager = new FixedUpdateManager();
            
            Singleton.RegisterSingleton(_updateManager);
            Singleton.RegisterSingleton(_fixedUpdateManager);
        }

        private void Update()
        {
            _updateManager.Update(default);

#if UNITY_EDITOR
            UpdatableNumber = _updateManager.Count;
            FixedUpdatableNumber = _fixedUpdateManager.Count;
#endif
        }

        private void FixedUpdate()
        {
            _fixedUpdateManager.Update(default);
        }
    }
    
    public interface IUpdatable<T>
    {
        void Update(T t);
    }

    public interface IUpdatable : IUpdatable<VOID>{}
    public struct VOID{}

    public abstract class UpdateManager<T> : IUpdatable
    {
        public abstract class System : IUpdatable<T>
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            static void RuntimeInit()
            {
                Manager = null;
                _systemDict.Clear();
            }
            
            public static UpdateManager<T> Manager;

            private static SortedDictionary<Type, Func<int>> _systemDict = new SortedDictionary<Type, Func<int>>(new HashComparer());

            public event Action OnRegister;
            public event Action OnUnregister;
            
            private Type _systemType;
            private int _registeredOrder;
            
            public Func<int> UpdateOrderGetter
            {
                get => _systemDict[_systemType];
                set => _systemDict[_systemType] = value;
            }

            protected System()
            {
                _systemType = GetType();
                
                if (!_systemDict.ContainsKey(_systemType))
                {
                    _systemDict.Add(_systemType, () => 0);
                }
            }

            public abstract void Update(T t);

            public void UpdateAfter<TOtherSystem>() where TOtherSystem : System
            {
                UpdateAfter(typeof(TOtherSystem));
            }

            public void UpdateBefore<TOtherSystem>() where TOtherSystem : System
            {
                UpdateBefore(typeof(TOtherSystem));
            }
            
            public void UpdateAfter(Type other)
            {
                if (_systemDict.ContainsKey(other))
                {
                    _systemDict[_systemType] = () => _systemDict[other]() + 1;
                    return;
                }
                
                _systemDict[_systemType] = () =>
                {
                    if (_systemDict.ContainsKey(other))
                    {
                        _systemDict[_systemType] = () => _systemDict[other]() + 1;
                        return UpdateOrderGetter();
                    }

                    return 0;
                };
            }

            public void UpdateBefore(Type other)
            {
                if (_systemDict.ContainsKey(other))
                {
                    _systemDict[_systemType] = () => _systemDict[other]() - 1;
                    return;
                }
                
                _systemDict[_systemType] = () =>
                {
                    if (_systemDict.ContainsKey(other))
                    {
                        _systemDict[_systemType] = () => _systemDict[other]() - 1;
                        return UpdateOrderGetter();
                    }

                    return 0;
                };
            }

            public void Register()
            {
                Manager.Register(this, UpdateOrderGetter());
                _registeredOrder = UpdateOrderGetter();
                OnRegister?.Invoke();
            }

            public void Unregister()
            {
                Manager.Unregister(this);
                OnUnregister?.Invoke();
            }

            protected void Reregister()
            {
                if (_registeredOrder != UpdateOrderGetter())
                {
                    Unregister();
                    Register();
                }
            }
            
            private class HashComparer : IComparer<Type>
            {
                public int Compare(Type x, Type y)
                {
                    if (ReferenceEquals(x, y))
                    {
                        return 0;
                    }

                    if (ReferenceEquals(null, y))
                    {
                        return 1;
                    }

                    if (ReferenceEquals(null, x))
                    {
                        return -1;
                    }

                    int nameComparison = string.Compare(x.Name, y.Name, StringComparison.Ordinal);
                    if (nameComparison != 0)
                    {
                        return nameComparison;
                    }

                    return x.GUID.CompareTo(y.GUID);
                }
            }
        }
        
        protected class HashComparer : IComparer<IUpdatable<T>>
        {
            public int Compare(IUpdatable<T> x, IUpdatable<T> y)
            {
                return y.GetHashCode() - x.GetHashCode();
            }
        }

        protected SortedList<int, SortedDictionary<IUpdatable<T>, IUpdatable<T>>> _updatableList = new SortedList<int, SortedDictionary<IUpdatable<T>, IUpdatable<T>>>();
        
        protected List<(int, IUpdatable<T>)> _addList = new List<(int, IUpdatable<T>)>();
        protected List<(int, IUpdatable<T>)> _removeList = new List<(int, IUpdatable<T>)>();

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

        public UpdateManager()
        {
            System.Manager = this;
            _exceptionMsg = $"{GetType().Name}.Update() 중에 문제가 발생했습니다. 해당 객체를 리스트에서 제외합니다.\n";
        }

        /// <summary>
        /// 작은 order 값부터 update됩니다
        /// </summary>
        public void Register(IUpdatable<T> updatable, int order = 0)
        {
            _addList.Add((order, updatable));
        }

        public void Unregister(IUpdatable<T> updatable, int order = 0)
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
        
        public void Update(VOID @void)
        {
            AddUpdatables();

            SetDelta();
            
            UpdateElements();
            
            RemoveUpdatables();
        }
        
        private void AddUpdatables()
        {
            foreach (var updatable in _addList)
            {
                if (!_updatableList.TryGetValue(updatable.Item1, out var dict))
                {
                    _updatableList.Add(updatable.Item1, new (new HashComparer()));
                }
                
                _updatableList[updatable.Item1].Add(updatable.Item2, updatable.Item2);
            }
            _addList.Clear();
        }

        private void RemoveUpdatables()
        {
            foreach (var updatable in _removeList)
            {
                if (_updatableList.TryGetValue(updatable.Item1, out var dict))
                {
                    dict.Remove(updatable.Item2);
                    if (dict.Count == 0)
                    {
                        _updatableList.Remove(updatable.Item1);
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
                        updatable.Value.Update(_updateDelta);
                    }
                    catch (Exception e)
                    {
                        Printer.Print(_exceptionMsg + e, logLevel:LogLevel.Error);
                        _removeList.Add((orderListPair.Key, updatable.Value));
                    }
                }
            }
        }

        /// <summary>
        /// Must Set _updateDelta here
        /// </summary>
        protected abstract void SetDelta();
    }

    public class UpdateManager : UpdateManager<float>
    {
        protected override void SetDelta()
        {
            _updateDelta = Time.deltaTime;
        }
    }

    public class FixedUpdateManager : UpdateManager<VOID>
    {
        protected override void SetDelta()
        {
            
        }
    }
}