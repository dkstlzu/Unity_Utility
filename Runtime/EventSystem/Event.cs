using System;

namespace dkstlzu.Utility
{
    public interface IEvent
    {
        Enum EventCode { get; }
    }
    
    [Serializable]
    public class Event : IEvent
    {
        public Enum EventCode{get; protected set;}

        public Event(Enum name)
        {
            EventCode = name;
        }
    }
}
