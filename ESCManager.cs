using System;

namespace Utility
{
    public class ESCManager : Singleton<ESCManager>
    {
        private PriorityQueue<Action> PQ = new PriorityQueue<Action>();
        public void ESC()
        {
            PriorityQueue<Action>.Item item = PQ.Pop();
            if (item != null) item.Element();
        }

        public void AddItem(string name, Action action, int priority)
        {
            PQ.AddItem(name, action, priority);
        }

        public void RemoveItem(string name)
        {
            PQ.RemoveItem(name);
        }
    }
}