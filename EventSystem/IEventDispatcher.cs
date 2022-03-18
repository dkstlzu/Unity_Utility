namespace Utility.EventSystem
{
    public interface IEventDispatcher
    {
        IEvent Event{get; set;}
        void Notify();
        void Notify(IEvent Event);
        void AddListener(IEventListener eventListener);
        void RemoveListener(IEventListener eventListener);
    }
}
