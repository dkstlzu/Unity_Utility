using System.Collections.Generic;
using UnityEngine;

namespace Utility.EventSystem
{
    [System.Serializable]
    public class EventDispatcher : IEventDispatcher
    {
        public IEvent Event {get; set;}
        protected List<IEventListener> ListenerList = new List<IEventListener>();
        protected Stack<IEvent> EventStack = new Stack<IEvent>();
        public void Notify()
        {
            if (Event == null)
            {
                Debug.LogWarning("No Assigned Default Event on EventDispatcher");
                return;
            }
            
            foreach (IEventListener listener in ListenerList)
            {
                listener.OnEvent(Event);
            }
        }
        public void Notify(IEvent e)
        {
            foreach (IEventListener listener in ListenerList)
            {
                listener.OnEvent(e);
            }
        }

        public void AddListener(IEventListener listener)
        {
            if (!ListenerList.Contains(listener))
                ListenerList.Add(listener);
        }
        
        public void RemoveListener(IEventListener listener)
        {
            ListenerList.Remove(listener);
        }
        
    }
}
