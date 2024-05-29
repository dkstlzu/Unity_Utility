using System.Collections.Generic;

namespace dkstlzu.Utility
{
    public interface IEventListener
    {
        void OnEvent(IEvent @event);
    }
    
    public interface IEventDispatcher
    {
        void Notify(IEvent @event);
        void AddListener(IEventListener eventListener);
        void RemoveListener(IEventListener eventListener);
    }
    
    [System.Serializable]
    public class EventDispatcher : IEventDispatcher
    {
        protected List<IEventListener> ListenerList = new List<IEventListener>();
        protected Stack<IEvent> EventStack = new Stack<IEvent>();

        public void Notify(IEvent @event)
        {
            foreach (IEventListener listener in ListenerList)
            {
                listener.OnEvent(@event);
            }
            
            EventStack.Push(@event);
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
