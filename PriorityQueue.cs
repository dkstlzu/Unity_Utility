using System.Collections.Generic;
using System;

namespace Utility
{
    public class PriorityQueue<T>
    {
        private Dictionary<string, Item> ItemDict = new Dictionary<string, Item>();
        public struct Item
        {
            public T Element;
            public int Priority;
        }

        public void AddItem(string name, Item item)
        {
            ItemDict.Add(name, item);
        }

        public void AddItem(string name, T element, int priority)
        {
            ItemDict.Add(name, new Item(){Element = element, Priority = priority});
        }

        public void RemoveItem(string name)
        {
            ItemDict.Remove(name);
        }

        public Item Pop()
        {
            var enumerator = ItemDict.Values.GetEnumerator();
            int priorityTemp = -1;
            Item targetItem = default(Item);

            while(enumerator.MoveNext())
            {
                Item Item = enumerator.Current;
                if (priorityTemp < Item.Priority)
                {
                    targetItem = Item;
                    priorityTemp = Item.Priority;
                }
            }

            return targetItem;
        }
    }
}