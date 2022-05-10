using System;

namespace Utility
{
    public class ESCManager
    {
        private PriorityQueue<Action> PQ = new PriorityQueue<Action>();
        public void ESC()
        {
            PQ.Pop().Element();
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