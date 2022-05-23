using System.Collections.Generic;
using System;

namespace Utility
{
    public class PriorityQueue<T>
    {
        private Dictionary<string, Item> ItemDict = new Dictionary<string, Item>();
        public class Item
        {
            public T Element;
            public int Priority;

            public Item(){}
            public Item(T element, int priority)
            {
                Element = element;
                Priority = priority;
            }
        }

        public void AddItem(string name, Item item)
        {
            if (!ItemDict.ContainsKey(name))
                ItemDict.Add(name, item);
        }

        public void AddItem(string name, T element, int priority)
        {
            AddItem(name, new Item(element, priority));
        }

        public void RemoveItem(string name)
        {
            if (ItemDict.ContainsKey(name))
                ItemDict.Remove(name);
        }

        public Item Pop()
        {
            var keyEnumerator = ItemDict.Keys.GetEnumerator();
            var valueEnumerator = ItemDict.Values.GetEnumerator();
            int priorityTemp = -1;
            string targetKey = string.Empty;
            Item targetItem = null;

            while(keyEnumerator.MoveNext() && valueEnumerator.MoveNext())
            {
                if (priorityTemp < valueEnumerator.Current.Priority)
                {
                    targetKey = keyEnumerator.Current;
                    targetItem = valueEnumerator.Current;
                    priorityTemp = valueEnumerator.Current.Priority;
                }
            }

            RemoveItem(targetKey);
            return targetItem;
        }
    }
}