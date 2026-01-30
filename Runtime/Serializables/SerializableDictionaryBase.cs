using System.Collections.Generic;
using UnityEngine;
using System;

namespace dkstlzu.Utility
{
    [Serializable]
    public class SerializeDictionaryBase<K, V> : Dictionary<K, V>, ISerializationCallbackReceiver
    {
        [SerializeField]
        List<K> keys = new List<K>();

        [SerializeField]
        List<V> values = new List<V>();

        public void OnBeforeSerialize()
        {
            for (int i = 0; i < keys.Count; i++)
            {
                if (!ContainsKey(keys[i]))
                {
                    keys.RemoveAt(i);
                    values.RemoveAt(i);
                    i--;
                }
            }

            foreach (KeyValuePair<K, V> pair in this)
            {
                if (!keys.Contains(pair.Key))
                {
                    keys.Add(pair.Key);
                    values.Add(pair.Value);
                }
            }
        }

        public void OnAfterDeserialize()
        {
            this.Clear();

            for (int i = 0, icount = keys.Count; i < icount; ++i)
            {
                this.TryAdd(keys[i], values[i]);
            }
        }
    }
}