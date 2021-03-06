using System;

namespace dkstlzu.Utility.EventSystem
{
    [System.Serializable]
    public class Event : IEvent
    {
        public Enum eventCode{get; protected set;}

        public Event(Enum name)
        {
            eventCode = name;
        }
    }
}
