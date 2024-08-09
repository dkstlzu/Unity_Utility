using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace dkstlzu.Utility
{
    [Serializable]
    public class StateMachine<T> : IFrameUpdatable where T : Enum
    {
        [SerializeField]
        private T _currentState = default;
        public T Current => _currentState;
        public Action CurrentAction => _eventDict[StateMachine.EventType.Stay][_currentState];

        public Action OnUpdate;
        private Dictionary<StateMachine.EventType, Dictionary<T, Action>> _eventDict;

        public StateMachine()
        {
            InitDict();
        }

        public void InitDict()
        {
            _eventDict = new Dictionary<StateMachine.EventType, Dictionary<T, Action>>();
            
            foreach (StateMachine.EventType eventType in Enum.GetValues(typeof(StateMachine.EventType)))
            {
                _eventDict.Add(eventType, new Dictionary<T, Action>());
                foreach (T t in Enum.GetValues(typeof(T)))
                {
                    _eventDict[eventType].Add(t, delegate { });
                }
            }
        }

        public void FrameUpdate(float delta)
        {
            if (_eventDict[StateMachine.EventType.Stay].TryGetValue(_currentState, out Action action))
            {
                action?.Invoke();                
            }
            OnUpdate?.Invoke();
        }
        
        public void AddEnterEvent(T t, Action action)
        {
            AddEvent(_eventDict[StateMachine.EventType.Enter], t, action);
        }

        public void RemoveEnterEvent(T t, Action action)
        {
            RemoveEvent(_eventDict[StateMachine.EventType.Enter], t, action);
        }        
        public void AddStayEvent(T t, Action action)
        {
            AddEvent(_eventDict[StateMachine.EventType.Stay], t, action);
        }

        public void RemoveStayEvent(T t, Action action)
        {
            RemoveEvent(_eventDict[StateMachine.EventType.Stay], t, action);

        }        
        public void AddExitEvent(T t, Action action)
        {
            AddEvent(_eventDict[StateMachine.EventType.Exit], t, action);
        }

        public void RemoveExitEvent(T t, Action action)
        {
            RemoveEvent(_eventDict[StateMachine.EventType.Exit], t, action);
        }

        private void AddEvent(Dictionary<T, Action> dict, T t, Action action)
        {
            dict[t] += action;
        }

        private void RemoveEvent(Dictionary<T, Action> dict, T t, Action action)
        {
            dict[t] -= action;
        }

        public void ChangeTo(T t, bool ignoreEvents)
        {
            if (isSameState(t, _currentState)) return;
            
            if (!ignoreEvents)
                _eventDict[StateMachine.EventType.Exit]?[_currentState]?.Invoke();

            _currentState = t;
            
            if (!ignoreEvents)
                _eventDict[StateMachine.EventType.Enter]?[_currentState]?.Invoke();
        }

        bool isSameState(T t1, T t2)
        {
            return EqualityComparer<T>.Default.Equals(t1, t2);
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
    public class StateMachine : IFrameUpdatable
    {
        public enum EventType
        {
            Enter,
            Stay,
            Exit,
        }
        
        [SerializeField]
        private string _currentState = string.Empty;
        public string Current => _currentState;
        public Action CurrentAction => _eventDict[EventType.Stay][_currentState];

        public Action OnUpdate;
        private Dictionary<EventType, Dictionary<string, Action>> _eventDict;

        public StateMachine(IEnumerable<string> keys)
        {
            Init(keys);
        }

        public void Init(IEnumerable<string> keys)
        {
            InitDict();
            var e = keys.GetEnumerator();

            while (e.MoveNext())
            {
                AddState(e.Current);
            }
            
            e.Dispose();
        }

        public void InitDict()
        {
            _eventDict = new Dictionary<EventType, Dictionary<string, Action>>();
            
            foreach (EventType eventType in Enum.GetValues(typeof(EventType)))
            {
                _eventDict.TryAdd(eventType, new Dictionary<string, Action>());
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
        
        private void AddEvent(Dictionary<string, Action> dict, string key, Action action)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, delegate { });
            }

            dict[key] += action;
        }
        
        private void RemoveEvent(Dictionary<string, Action> dict, string key, Action action)
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

            if (!ignoreEvents && _eventDict[EventType.Exit].TryGetValue(_currentState, out Action exitAction))
            {
                exitAction?.Invoke();
            }

            _currentState = key;

            if (!ignoreEvents && _eventDict[EventType.Enter].TryGetValue(_currentState, out Action enterAction))
            {
                enterAction?.Invoke();
            }
        }
    }
}