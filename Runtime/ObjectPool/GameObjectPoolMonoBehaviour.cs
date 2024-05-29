using System;
using UnityEngine;

namespace dkstlzu.Utility
{
    [AddComponentMenu("GameObjectPool")]
    public class GameObjectPoolMonoBehaviour : MonoBehaviour
    {
        public GameObject Prefab;
        public int InitialNumber;

        private void Start()
        {
            GameObjectPool.GetOrCreate(Prefab, InitialNumber);
            Destroy(this);
        }
    }
}