using System;

namespace Utility
{
    public class ESCManager : Singleton<ESCManager>
    {
        class ActionPriorityQueue : PriorityQueue<Action> {}
        private ActionPriorityQueue PQ = new ActionPriorityQueue();
        public void ESC()
        {
            PriorityQueue<Action>.Item item = PQ.Pop();
            if (item != null) item.Element();
        }

        public void AddItem(string name, Action action, int priority)
        {
            PQ.AddItem(name, action, priority);
        }

        public PriorityQueue<Action>.Item RemoveItem(string name)
        {
            return PQ.RemoveItem(name);
        }
    }
}