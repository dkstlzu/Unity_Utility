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

        public Action OnUpdate;
        public Action<string> OnStateChanged;
        
        [SerializeField]
        protected string[] _keys;
        
        protected Dictionary<EventType, Dictionary<string, Action>> _eventDict;

        public Dictionary<string, Action>.KeyCollection StateNames => _eventDict[EventType.Enter].Keys;

        public StateMachine(IEnumerable<string> keys)
        {
            Init(keys);
        }

        public void Init(IEnumerable<string> keys)
        {
            Reset();
            
            _keys = keys.ToArray();
            
            InitDict(keys);

            var en = keys.GetEnumerator();

            if (en.MoveNext())
            {
                _currentState = en.Current;
            }
            
            en.Dispose();
        }

        public void InitDict(IEnumerable<string> keys)
        {
            _eventDict = new Dictionary<EventType, Dictionary<string, Action>>();
                
            foreach (EventType eventType in Enum.GetValues(typeof(EventType)))
            {
                _eventDict.Add(eventType, new Dictionary<string, Action>());

                foreach (var key in keys)
                {
                    AddState(key);
                }
            }
        }

        public void Reset()
        {
            _currentState = String.Empty;
            _keys = Array.Empty<string>();

            if (_eventDict != null)
            {
                _eventDict.Clear();
            }
        }
        
        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            InitDict(_keys);
        }
        
        public void FrameUpdate(float delta)
        {
            if (_eventDict[EventType.Stay].TryGetValue(_currentState, out Action action))
            {
                action?.Invoke();                
            }
            OnUpdate?.Invoke();
        }
        
        public void AddEnterEvent(string key, Action action)
        {
            AddEvent(_eventDict[EventType.Enter], key, action);
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
        
        public void AddExitEvent(string key, Action action)
        {
            AddEvent(_eventDict[EventType.Exit], key, action);
        }

        public void RemoveExitEvent(string key, Action action)
        {
            RemoveEvent(_eventDict[EventType.Exit], key, action);
        }

        private void AddState(string key)
        {
            foreach (var eventTypePair in _eventDict)
            {
                if (!eventTypePair.Value.ContainsKey(key))
                {
                    eventTypePair.Value.Add(key, delegate { });
                }
            }
        }
        
        protected void AddEvent(Dictionary<string, Action> dict, string key, Action action)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, delegate { });
            }

            dict[key] += action;
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

        public void ChangeTo(string key, bool ignoreEvents)
        {
            if (key == _currentState) return;

            if (!ignoreEvents)
            {
                _eventDict[EventType.Exit]?[_currentState]?.Invoke();
            }

            _currentState = key;

            if (!ignoreEvents)
            {
                _eventDict[EventType.Enter]?[_currentState]?.Invoke();
            }
            
            OnStateChanged?.Invoke(_currentState);
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

        public Action<T> OnEnumStateChanged;

        public StateMachine() : base(Enum.GetNames(typeof(T)))
        {
            OnEnumStateChanged += e => OnStateChanged?.Invoke(e.ToString());
            _currentState = Enum.ToObject(typeof(T), 0).ToString();
        }

        public void AddEnterEvent(T state, Action action)
        {
            AddEnterEvent(state.ToString(), action);
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
        public void AddExitEvent(T state, Action action)
        {
            AddExitEvent(state.ToString(), action);
        }

        public void RemoveExitEvent(T state, Action action)
        {
            RemoveExitEvent(state.ToString(), action);
        }

        public void ChangeTo(T to, bool ignoreEvents)
        {
            if (isSameState(CurrentEnum, to)) return;
            
            ChangeTo(to.ToString(), ignoreEvents);
            
            OnEnumStateChanged?.Invoke(CurrentEnum);
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