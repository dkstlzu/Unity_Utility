namespace Utility.EventSystem
{
    public interface IEventListener
    {
        void OnEvent(IEvent e);
    }
}
