using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace dkstlzu.Utility
{
    [Serializable]
    public class StateMachine : IFrameUpdatable, ISerializationCallbackReceiver
    {
        public enum EventType
        {
            Enter,
            Stay,
            Exit,
        }
        
        [SerializeField]
        protected string _currentState = string.Empty;
        public string Current => _currentState;
        public Action CurrentAction => _eventDict[EventType.Stay][_currentState];

        [NonSerialized]
        public Action OnUpdate;
        [NonSerialized]
        public Action<string, string> OnStateChanged;
        
        [SerializeField]
        protected string[] _keys;

        public List<string> Keys => _keys == null ? new List<string>() : new List<string>(_keys);
        
        protected Dictionary<EventType, Dictionary<string, Action>> _eventDict;
        protected Dictionary<string, bool> _transferableDict;

        public bool EnableLog;
        
        public StateMachine()
        {
            _keys = Array.Empty<string>();
        }
        
        public StateMachine(IEnumerable<string> keys)
        {
            Init(keys);
        }

        public void Init(IEnumerable<string> keys)
        {
            Reset();

            InitKeys(keys);
            InitDict(keys);
        }

        public void InitKeys(IEnumerable<string> keys)
        {
            _keys = keys.ToArray();

            if (_keys.Length > 0)
            {
                _currentState = _keys[0];
            }
            else
            {
                _currentState = String.Empty;
            }
        }

        public void InitDict(IEnumerable<string> keys)
        {
            if (_eventDict == null)
            {
                _eventDict = new Dictionary<EventType, Dictionary<string, Action>>();
            }
            else
            {
                _eventDict.Clear();
            }
                
            foreach (EventType eventType in Enum.GetValues(typeof(EventType)))
            {
                _eventDict.Add(eventType, new Dictionary<string, Action>());
            }
            
            if (_transferableDict == null)
            {
                _transferableDict = new Dictionary<string, bool>();
            }
            else
            {
                _transferableDict.Clear();
            }

            var keyList = new List<string>(keys);
            
            for (int i = 0; i < keyList.Count; i++)
            {
                for (int j = 0; j < keyList.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    
                    _transferableDict.Add(GetTransferableKey(keyList[i], keyList[j]), true);
                }
            }
        }
        
        public virtual void Reset()
        {
            _currentState = String.Empty;
            _keys = Array.Empty<string>();

            if (_eventDict != null)
            {
                _eventDict.Clear();
            }

            OnStateChanged = null;
        }
        
        public virtual void OnBeforeSerialize()
        {
        }

        public virtual void OnAfterDeserialize()
        {
            if (!_keys.Contains(_currentState))
            {
                Init(_keys);
            }
            else
            {
                InitDict(_keys);
            }
        }
        
        public void FrameUpdate(float delta)
        {
            if (_eventDict[EventType.Stay].TryGetValue(_currentState, out Action action))
            {
                action?.Invoke();                
            }
            OnUpdate?.Invoke();
        }
        
        public void AddEnterEvent(string key, Action action, bool invokeIfAlready = false)
        {
            AddEvent(_eventDict[EventType.Enter], key, action);

            if (invokeIfAlready && _currentState == key)
            {
                action?.Invoke();
            }
        }

        public void RemoveEnterEvent(string key, Action action)
        {
            RemoveEvent(_eventDict[EventType.Enter], key, action);
        }        
        public void AddStayEvent(string key, Action action)
        {
            AddEvent(_eventDict[EventType.Stay], key, action);
        }

        public void RemoveStayEvent(string key, Action action)
        {
            RemoveEvent(_eventDict[EventType.Stay], key, action);
        }        
        
        public void AddExitEvent(string key, Action action, bool invokeIfAlready = false)
        {
            AddEvent(_eventDict[EventType.Exit], key, action);

            if (invokeIfAlready && _currentState != key)
            {
                action?.Invoke();
            }
        }

        public void RemoveExitEvent(string key, Action action)
        {
            RemoveEvent(_eventDict[EventType.Exit], key, action);
        }

        protected void AddEvent(Dictionary<string, Action> dict, string key, Action action)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, action);
            }
            else
            {
                dict[key] += action;
            }
        }
        
        protected void RemoveEvent(Dictionary<string, Action> dict, string key, Action action)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] -= action;
            }

            if (dict[key].GetInvocationList().Length == 0)
            {
                dict.Remove(key);
            }
        }

        public void ClearEvent(string key)
        {
            foreach (var pair in _eventDict)
            {
                pair.Value.Remove(key);
            }
        }

        public void ClearDict()
        {
            InitDict(_keys);
        }

        public void SetTransferable(string from, string to, bool movable)
        {
            _transferableDict[GetTransferableKey(from, to)] = movable;
        }

        protected string GetTransferableKey(string from, string to) => $"{from}.{to}";

        public void ChangeTo(string key, bool ignoreEvents = false)
        {
            if (!_keys.Contains(key))
            {
                Printer.Print($"{key} is not valid state of StateMachine", logLevel:LogLevel.Error, priority:1);
                return;
            }
            
            if (key == _currentState) return;
            if (!_transferableDict[GetTransferableKey(_currentState, key)])
            {
                Printer.Print($"from {_currentState} to {key} transition is impossible because of Transferable Setting", logLevel:LogLevel.Warning);
                return;
            }

            if (!ignoreEvents)
            {
                if (_eventDict[EventType.Exit].ContainsKey(_currentState))
                {
                    if (EnableLog)
                    {
                        foreach (var call in _eventDict[EventType.Exit]?[_currentState]?.GetInvocationList())
                        {
                            Printer.Print($"Exit {_currentState} Call {call.Target}.{call.Method.Name}", priority:-1);
                        }
                    }

                    _eventDict[EventType.Exit]?[_currentState]?.Invoke();
                }
            }

            if (EnableLog)
            {
                Printer.Print($"Change State from {_currentState} to {key}");
            }
            var previous = _currentState;
            _currentState = key;
            OnStateChanged?.Invoke(previous, key);

            if (!ignoreEvents)
            {
                if (_eventDict[EventType.Enter].ContainsKey(_currentState))
                {
                    if (EnableLog)
                    {
                        foreach (var call in _eventDict[EventType.Enter]?[_currentState]?.GetInvocationList())
                        {
                            Printer.Print($"Enter {_currentState} Call {call.Target}.{call.Method.Name}", priority:-1);
                        }
                    }

                    _eventDict[EventType.Enter]?[_currentState]?.Invoke();
                }
            }
        }

        public void SimulateChange(string from, string to)
        {
            if (from == to) return;
            
            _eventDict[EventType.Exit]?[from]?.Invoke();
            _eventDict[EventType.Enter]?[to]?.Invoke();
        }
        
        public void PrintInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var pair in _eventDict)
            {
                stringBuilder.AppendLine(pair.Key.ToString());
                foreach (var innerPair in pair.Value)
                {
                    stringBuilder.Append(innerPair.Key + " " + innerPair.Value.GetInvocationList().Length + ":");
                }

                stringBuilder.AppendLine();
            }
            
            Printer.Print(stringBuilder.ToString());
        }
    }
    
    [Serializable]
    public class StateMachine<T> : StateMachine where T : struct, Enum
    {
        public T CurrentEnum
        {
            get => Enum.Parse<T>(_currentState);
            protected set => _currentState = value.ToString();
        }

        [NonSerialized]
        public Action<T, T> OnEnumStateChanged;

        public StateMachine() : base(Enum.GetNames(typeof(T)))
        {
            OnStateChanged += (from, to) => OnEnumStateChanged?.Invoke(Enum.Parse<T>(from), Enum.Parse<T>(to));
            _currentState = Enum.ToObject(typeof(T), 0).ToString();
        }

        public void Init()
        {
            InitKeys(Enum.GetNames(typeof(T)));
            InitDict(_keys);
        }
        
        public void Init(T startState)
        {
            Init();
            CurrentEnum = startState;
        }

        public override void Reset()
        {
            base.Reset();

            OnEnumStateChanged = null;

            var keys = Enum.GetNames(typeof(T));
            InitKeys(keys);
            InitDict(keys);
        }

        public override void OnBeforeSerialize()
        {
            if (!IsValid())
            {
                Init();
            }
        }

        public override void OnAfterDeserialize()
        {
            if (!IsValid())
            {
                Init();
            }
        }

        public bool IsValid()
        {
            var validNames = Enum.GetNames(typeof(T));

            if (validNames.Length != _keys.Length)
            {
                return false;
            }

            foreach (var key in _keys)
            {
                if (!validNames.Contains(key))
                {
                    return false;
                }
            }
            
            if (!validNames.Contains(_currentState))
            {
                return false;
            }

            return true;
        }
        
        public void AddEnterEvent(T state, Action action, bool invokeIfAlready = false)
        {
            AddEnterEvent(state.ToString(), action, invokeIfAlready);
        }

        public void RemoveEnterEvent(T state, Action action)
        {
            RemoveEnterEvent(state.ToString(), action);
        }        
        public void AddStayEvent(T state, Action action)
        {
            AddStayEvent(state.ToString(), action);
        }

        public void RemoveStayEvent(T state, Action action)
        {
            RemoveStayEvent(state.ToString(), action);

        }        
        public void AddExitEvent(T state, Action action, bool invokeIfAlready = false)
        {
            AddExitEvent(state.ToString(), action, invokeIfAlready);
        }

        public void RemoveExitEvent(T state, Action action)
        {
            RemoveExitEvent(state.ToString(), action);
        }

        public void ClearEvent(T state)
        {
            ClearEvent(state.ToString());
        }
        
        public void SetTransferable(T from, T to, bool movable)
        {
            SetTransferable(from.ToString(), to.ToString(), movable);
        }
        
        public void ChangeTo(T to, bool ignoreEvents = false)
        {
            if (isSameState(CurrentEnum, to)) return;
            
            ChangeTo(to.ToString(), ignoreEvents);
        }

        public void SimulateChange(T from, T to)
        {
            if (isSameState(from, to)) return;

            SimulateChange(from.ToString(), to.ToString());
        }

        bool isSameState(T t1, T t2)
        {
            return EqualityComparer<T>.Default.Equals(t1, t2);
        }
    }
}