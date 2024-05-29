using System;
using System.Collections.Generic;

namespace dkstlzu.Utility
{
    public class EventSystem : Singleton<EventSystem>
    {
        private readonly Dictionary<Enum, IEventDispatcher> _eventDispatcherDict = new Dictionary<Enum, IEventDispatcher>();
        
        public void Invoke(Enum eventEnum, IEvent e)
        {
            if (_eventDispatcherDict.TryGetValue(eventEnum, out IEventDispatcher dispatcher))
            {
                dispatcher.Notify(e);
            }
        }
        
        public void InvokeAll(IEvent e)
        {
            foreach (KeyValuePair<Enum, IEventDispatcher> pair in _eventDispatcherDict)
            {
                pair.Value.Notify(e);
            }
        }

        public IEventDispatcher[] AddEventDispatcher(Enum[] eventEnums)
        {
            List<IEventDispatcher> dispatchers = new List<IEventDispatcher>();
            
            for (int i = 0; i < eventEnums.Length; i++)
            {
                dispatchers.Add(AddEventDispatcher(eventEnums[i]));
            }

            return dispatchers.ToArray();
        }

        public IEventDispatcher AddEventDispatcher(Enum eventEnum)
        {
            _eventDispatcherDict.Add(eventEnum, new EventDispatcher());
            return _eventDispatcherDict[eventEnum];
        }
        
        public IEventDispatcher AddEventDispatcher(Enum eventEnum, IEventListener listener) => AddEventDispatcher(eventEnum, new[] { listener });
        public IEventDispatcher AddEventDispatcher(Enum eventEnum, IEventListener[] listeners)
        {
            if (_eventDispatcherDict.TryGetValue(eventEnum, out IEventDispatcher found))
            {
                return found;
            }

            var newDispatcher = new EventDispatcher();
            _eventDispatcherDict.Add(eventEnum, newDispatcher);

            foreach (IEventListener listener in listeners)
            {
                newDispatcher.AddListener(listener);
            }

            return newDispatcher;
        }

        public void RemoveEventDispatcher(Enum eventEnum)
        {
            _eventDispatcherDict.Remove(eventEnum);
        }

        public IEventDispatcher GetEventDispatcher(Enum eventEnum)
        {
            _eventDispatcherDict.TryGetValue(eventEnum, out IEventDispatcher dispatcher);
            return dispatcher;
        }

        public void AddEventListener(Enum eventEnum, IEventListener listener)
        {
            if (!_eventDispatcherDict.TryGetValue(eventEnum, out IEventDispatcher dispatcher))
            {
                AddEventDispatcher(eventEnum);
            }
            
            _eventDispatcherDict[eventEnum].AddListener(listener);
        }

        public void RemoveEventListener(Enum eventEnum, IEventListener listener)
        {
            IEventDispatcher dispatcher = GetEventDispatcher(eventEnum);
            if (dispatcher == null)
            {
                return;
            }

            dispatcher.RemoveListener(listener);
        }
    }
}
