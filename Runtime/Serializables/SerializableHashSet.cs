using System;
using System.Collections.Generic;
using UnityEngine;

namespace dkstlzu.Utility
{
    [Serializable]
    public class SerializableHashSet<T> : HashSet<T>, ISerializationCallbackReceiver
    {
        public List<T> list = new List<T>();

        public SerializableHashSet() : base() { }
        public SerializableHashSet(System.Collections.Generic.IEnumerable<T> collection) : base(collection) { }
        public SerializableHashSet(System.Collections.Generic.IEnumerable<T> collection, System.Collections.Generic.IEqualityComparer<T> comparer) : base(collection, comparer) { }
        public SerializableHashSet(System.Collections.Generic.IEqualityComparer<T> comparer) : base(comparer) { }
        public SerializableHashSet(int capacity) : base(capacity) { }
        public SerializableHashSet(int capacity, System.Collections.Generic.IEqualityComparer<T> comparer) : base(capacity, comparer) { }
        protected SerializableHashSet(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        
        public void OnBeforeSerialize()
        {
            list.Clear();
            list.AddRange(this);
        }

        public void OnAfterDeserialize()
        {
            Clear();

            foreach (T t in list)
            {
                Add(t);
            }
        }
    }
}