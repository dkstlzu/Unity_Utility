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

        public Item RemoveItem(string name)
        {
            Item target = null;
            if (ItemDict.ContainsKey(name))
            {
                target = ItemDict[name];
                ItemDict.Remove(name);
            }
            return target;
        }

        public Item Pop()
        {
            var keyEnumerator = ItemDict.Keys.GetEnumerator();
            var valueEnumerator = ItemDict.Values.GetEnumerator();
            int priorityTemp = -1;
            string targetKey = string.Empty;

            while(keyEnumerator.MoveNext() && valueEnumerator.MoveNext())
            {
                if (priorityTemp < valueEnumerator.Current.Priority)
                {
                    targetKey = keyEnumerator.Current;
                    priorityTemp = valueEnumerator.Current.Priority;
                }
            }

            return RemoveItem(targetKey);
        }

        public static object Pop(PriorityQueue<object> queue1, PriorityQueue<object> queue2)
        {
            object obj1 = queue1.Pop();
            object obj2 = queue2.Pop();

            if (((Item)obj1).Priority >= ((Item)obj2).Priority)
            {
                return (Item)obj1;
            } else
            {
                return (Item)obj2;
            }
        }
    }
}