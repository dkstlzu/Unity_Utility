using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.EventSystem
{
    public class EventSystem : Singleton<EventSystem>
    {
        private Dictionary<Enum, IEventDispatcher> EventDispatcherDict = new Dictionary<Enum, IEventDispatcher>();
        private IEventDispatcher dispatcher;
        public void Invoke(Enum eventEnum)
        {
            if (EventDispatcherDict.TryGetValue(eventEnum, out dispatcher))
            {
                dispatcher.Notify();
            }
        }

        public void Invoke(Enum eventEnum, IEvent e)
        {
            if (EventDispatcherDict.TryGetValue(eventEnum, out dispatcher))
            {
                dispatcher.Notify(e);
            }
        }

        public void Invoke(IEvent e)
        {
            if (EventDispatcherDict.TryGetValue(e.eventCode, out dispatcher))
            {
                dispatcher.Notify(e);
            }
        }


        public void InvokeAll()
        {
            foreach (KeyValuePair<Enum, IEventDispatcher> pair in EventDispatcherDict)
            {
                pair.Value.Notify();
            }
        }
        
        public void InvokeAll(IEvent e)
        {
            foreach (KeyValuePair<Enum, IEventDispatcher> pair in EventDispatcherDict)
            {
                pair.Value.Notify(e);
            }
        }

        public IEventDispatcher AddEventDispatcher(Enum eventEnum, IEventListener listener)
        {
            return AddEventDispatcher(eventEnum, new Event(eventEnum), listener);
        }

        public IEventDispatcher AddEventDispatcher(Enum eventEnum, IEventListener[] listeners = null)
        {
            return AddEventDispatcher(eventEnum, new Event(eventEnum), listeners);
        }
        public IEventDispatcher AddEventDispatcher(Enum eventEnum, IEvent Event, IEventListener listener)
        {
            IEventListener[] listeners = new IEventListener[1] {listener};
            return AddEventDispatcher(eventEnum, Event, listeners);
        }

        public IEventDispatcher AddEventDispatcher(Enum eventEnum, IEvent Event, IEventListener[] listeners = null)
        {
            IEventDispatcher dispatcher;
            if (!EventDispatcherDict.TryGetValue(eventEnum, out dispatcher))
            {
                dispatcher = new EventDispatcher();
                dispatcher.Event = Event;
                EventDispatcherDict.Add(eventEnum, dispatcher);

                if (listeners != null)
                {
                    foreach (IEventListener listener in listeners)
                    {
                        dispatcher.AddListener(listener);
                    }
                }
            }
            return dispatcher;
        }


        public IEventDispatcher[] AddEventDispatcher(Enum[] eventEnums)
        {
            Event[] Events = new Event[eventEnums.Length];
            
            for (int i = 0; i < eventEnums.Length; i++)
            {
                Events[i] = new Event(eventEnums[i]);
            }

            return AddEventDispatcher(eventEnums, Events);
        }

        /// <summary>
        /// Multi EventDispatcher adder Length of eventEnums and Events must be same 
        /// </summary>
        /// <returns></returns>
        public IEventDispatcher[] AddEventDispatcher(Enum[] eventEnums, IEvent[] Events)
        {
            if (eventEnums.Length != Events.Length)
            {
                Debug.LogWarning("Wrong EventAdding. Check your code");
                return null;
            }

            IEventDispatcher[] dispatchers = new IEventDispatcher[eventEnums.Length];

            for (int i = 0; i < eventEnums.Length; i++)
            {
                dispatchers[i] = AddEventDispatcher(eventEnums[i], Events[i]);
            }
            return dispatchers;
        }

        public IEventDispatcher RemoveEventDispahtcer(Enum eventEnum)
        {
            IEventDispatcher dispatcher;
            if (EventDispatcherDict.TryGetValue(eventEnum, out dispatcher))
            {
                EventDispatcherDict.Remove(eventEnum);
                return dispatcher;
            }
            return null;
        }

        public IEventDispatcher GetEventDispatcher(Enum eventEnum)
        {
            IEventDispatcher dispatcher;
            EventDispatcherDict.TryGetValue(eventEnum, out dispatcher);
            return dispatcher;
        }

        public void AddEventListener(Enum eventEnum, IEventListener listener)
        {
            IEventDispatcher eventDispatcher = GetEventDispatcher(eventEnum);
            if (eventDispatcher == null)
            {
                Debug.LogError($"�־��� Enum '{eventEnum}'�� ���� EventDispatcher�� ��ϵ��� �ʾҽ��ϴ�.");
                return;
            }

            eventDispatcher.AddListener(listener);
        }

        public void RemoveEventListener(Enum eventEnum, IEventListener listener)
        {
            IEventDispatcher eventDispatcher = GetEventDispatcher(eventEnum);
            if (eventDispatcher == null)
            {
                Debug.LogError($"�־��� Enum'{eventEnum}'�� ���� EventDispatcher�� ��ϵ��� �ʾҽ��ϴ�.");
                return;
            }

            eventDispatcher.RemoveListener(listener);
        }

        public static void OnEvent(IEvent e, UnityEngine.Object obj, UnityEngine.Object[] param)
        {
            string eventcode = e.eventCode.ToString();
            MethodInfo mi = obj.GetType().GetMethod(eventcode);
            mi.Invoke(obj, param);
        }
    }
}
