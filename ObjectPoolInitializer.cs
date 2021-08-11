using UnityEngine;

namespace Utility
{
    public class ObjectPoolInitializer : MonoBehaviour
    {
        public PoolSettings ObjectPoolSetting = new PoolSettings{};
        public GameObject SourcePrefab;
        public GameObject ObjectPoolParentObject;

        void Awake()
        {
            ObjectPool pool;
            pool = ObjectPool.GetOrCreate(ObjectPoolSetting.PoolName, ObjectPoolParentObject);
            pool.SourceObject = SourcePrefab;
            pool.Init(ObjectPoolSetting);
        }
    }
}