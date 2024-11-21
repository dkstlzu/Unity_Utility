using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace dkstlzu.Utility
{
    [Serializable]
    public class DoubleList<T>
    {
        [Serializable]
        public class ListElement
        {
            public List<T> Element;

            public void Clear()
            {
                Element.Clear();
            }
            
            public static implicit operator List<T>(ListElement e) => e.Element;
            public static implicit operator ListElement(List<T> list) => new ListElement(){Element = list};
        }

        public List<ListElement> List = new List<ListElement>();

        public int Count => List.Count;

        public ListElement this[int index]
        {
            get => List[index];
            set => List[index] = value;
        }

        public void Add([CanBeNull] ListElement item)
        {
            List.Add(item);
        }

        public void AddRange([NotNull] IEnumerable<ListElement> collection)
        {
            List.AddRange(collection);
        }

        public void Clear()
        {
            List.Clear();
        }
    }
}