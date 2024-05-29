using System;
using System.Collections.Generic;
using UnityEngine;

namespace dkstlzu.Utility
{
    [AddComponentMenu("ObjectPool")]
    public class ObjectPoolMonoBehaviour : MonoBehaviour
    {
        private static GameObject _gameObject;

        public static GameObject GameObject
        {
            get
            {
                if (_gameObject == null)
                {
                    _gameObject = new GameObject("ObjectPools");
#if UNITY_EDITOR
                    _gameObject.AddComponent<ObjectPoolMonoBehaviour>();
#endif
                }

                return _gameObject;
            }
        }

        private void Awake()
        {
            if (_gameObject == null)
            {
                _gameObject = gameObject;
            }
        }

#if UNITY_EDITOR
        public List<GameObjectPool> GameObjectPools = new List<GameObjectPool>();
        public int ObjectPoolNumber;

        private void Start()
        {
            _gameObject = gameObject;

            GameObjectPools.Clear();
            foreach (var pool in ObjectPool.Pools)
            {
                if (pool is GameObjectPool p)
                {
                    GameObjectPools.Add(p);
                }
            }
            
            ObjectPool.OnPoolAdded += (pool) =>
            {
                if (pool is GameObjectPool p)
                {
                    GameObjectPools.Add(p);
                }
            };
        }

        private void Update()
        {
            ObjectPoolNumber = ObjectPool.Count;
        }
#endif
    }
}