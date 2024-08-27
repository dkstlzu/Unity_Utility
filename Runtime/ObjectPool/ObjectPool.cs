#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace dkstlzu.Utility
{
    /// <summary>
    /// ObjectPool Get, Return 함수 동작시에 원하는 작업을 끼워넣을 수 있는 인터페이스 입니다.
    /// </summary>
    public interface IObjectPoolable
    {
        void OnGet();
        void OnReturn();
    }

    /// <summary>
    /// ObjectPool의 Get, Return 함수를 정의합니다. 
    /// </summary>
    public interface IObjectPool<T>
    {
        T? Get();
        void Return(T t);
    }
    
    /// <summary>
    /// ObjectPool의 base클래스이며 각 ObjectPool 인스턴스에 대한 레퍼런스를 저장합니다.
    /// </summary>
    public abstract class ObjectPool
    {
        protected static Dictionary<int, ObjectPool> PoolDict = new Dictionary<int, ObjectPool>();
        public static IEnumerable<ObjectPool> Pools => PoolDict.Values;
        public static int Count => PoolDict.Count;
        public static event Action<ObjectPool> OnPoolAdded = delegate { };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RuntimeInit()
        {
            PoolDict.Clear();
            OnPoolAdded = delegate { };
        }

        protected ObjectPool(){}
        
        public static bool ContainsPool(string key) => ContainsPool(key.GetHashCode());
        public static bool ContainsPool(int key) => PoolDict.ContainsKey(key);
        
        public static bool TryAddPool(string key, ObjectPool pool) => TryAddPool(key.GetHashCode(), pool);
        public static bool TryAddPool(int hashKey, ObjectPool pool)
        {
            if (PoolDict.TryAdd(hashKey, pool))
            {
                OnPoolAdded?.Invoke(pool);
                return true;
            }

            return false;
        }

        public int Capacity { get; protected set; }
        public abstract int PoolingCount { get; }
        public int EnabledCount { get => Capacity - PoolingCount; }
        protected bool _initialized = false;

        public const int DEFAULT_CAPACITY = 16;

        protected void initialize(int initializeSize)
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            instantiate(Math.PowerOf2Ceiling(initializeSize));
        }
        
        public void Resize(int size)
        {
            int newCapacity = Math.PowerOf2Ceiling(size);
            
            if (newCapacity == Capacity) return;

            if (newCapacity < Capacity)
            {
                destroy(Capacity - newCapacity);
            }
            else
            {
                instantiate(newCapacity - Capacity);
            }
        }
        
        protected abstract void instantiate(int number);
        protected abstract void destroy(int number);
    }
    
    public class UnityObjectPool : Singleton<UnityObjectPool>
    {
    }

    /// <summary>
    /// Interface 로 강제되는 ObjectPool입니다.
    /// </summary>
    public abstract class ObjectPool<T> : ObjectPool, IObjectPool<T>
    {
        public abstract T? Get();
        public abstract void Return(T t);
        
        protected Queue<T> Q = new Queue<T>();
        public override int PoolingCount => Q.Count;
    }

    /// <summary>
    /// c#의 reference type class에 대한 ObjectPool입니다.
    /// </summary>
    [Serializable]
    public class ClassObjectPool<T> : ObjectPool<T> where T : class, new()
    {
        public static ClassObjectPool<T> GetOrCreate(int initialSize = -1)
        {
            if (PoolDict.TryGetValue(typeof(T).Name.GetHashCode(), out var pool))
            {
                return (ClassObjectPool<T>)pool;
            }

            var newPool = new ClassObjectPool<T>();
            newPool._initialCapacity = initialSize < 0 ? DEFAULT_CAPACITY : initialSize;
            TryAddPool(newPool.HashKey, newPool);
            newPool.initialize(newPool._initialCapacity);
        
            return newPool;
        }

        public string Key => typeof(T).Name;
        public int HashKey => Key.GetHashCode();

        [SerializeField] 
        private int _initialCapacity;

        public void Init()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            Q = new Queue<T>();
            instantiate(_initialCapacity);
        }

        protected override void instantiate(int number)
        {
            for (int i = 0; i < number; i++, Capacity++)
            {
                Q.Enqueue(new T());
            }
        }

        protected override void destroy(int number)
        {
            for (int i = 0; i < number && PoolingCount > 0; i++, Capacity--)
            {
                var t = Q.Dequeue();

                if (t is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        public override T? Get()
        {
            if (PoolingCount == 0)
            {
                return null;
            }

            T t = Q.Dequeue();
            
            if (t is IObjectPoolable poolable)
            {
                poolable.OnGet();
            }

            return t;
        }
        
        public override void Return(T t)
        {
            if (t is IObjectPoolable poolable)
            {
                poolable.OnReturn();
            }
            
            Q.Enqueue(t);
        }
    }
    
    /// <summary>
    /// unity의 MonoBehaviour에 대한 ObjectPool입니다.
    /// </summary>
    [Serializable]
    public class BehaviourObjectPool<T> : ObjectPool<T> where T : Behaviour
    {
        public static BehaviourObjectPool<T> GetOrCreate(int initialSize = -1)
        {
            if (PoolDict.TryGetValue(typeof(T).Name.GetHashCode(), out ObjectPool pool))
            {
                return (BehaviourObjectPool<T>)pool;
            }

            var newPool = new BehaviourObjectPool<T>(ObjectPoolMonoBehaviour.GameObject);
            newPool._initialCapacity = initialSize < 0 ? DEFAULT_CAPACITY : initialSize;
            TryAddPool(newPool.HashKey, newPool);
            newPool.initialize(newPool._initialCapacity);
            
            return newPool;
        }

        public string Key => typeof(T).Name;
        public int HashKey => Key.GetHashCode();
        
        [SerializeField] 
        private int _initialCapacity;
        
        [field:SerializeField]
        public GameObject Parent { get; private set; }
        public GameObject? gameObject { get; private set; }

        
        private BehaviourObjectPool(GameObject parentGameObject)
        {
            Parent = parentGameObject;
        }

        /// <summary>
        /// Serializable을 활용한 사용례에서 호출되어야만 하는 함수입니다.
        /// </summary>
        public void Init(Action<T>? initWith = null)
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            Q = new Queue<T>();
            instantiate(_initialCapacity);
            if (initWith != null)
            {
                foreach (var t in Q)
                {
                    initWith(t);
                }
            }
        }

        protected override void instantiate(int number)
        {
            if (Parent == null)
            {
                Parent = UnityObjectPool.GetOrCreate().gameObject;
            }

            if (gameObject == null)
            {
                gameObject = new GameObject($"{Key} ObjectPool");
                gameObject.transform.SetParent(Parent.transform);
            }
            
            gameObject.SetActive(false);
            
            for (int i = 0; i < number; i++, Capacity++)
            {
                var t = gameObject.AddComponent<T>();
                
                t.enabled = false;
                
                Q.Enqueue(t);
            }
            
            gameObject.SetActive(true);
        }

        protected override void destroy(int number)
        {
            for (int i = 0; i < number && PoolingCount > 0; i++, Capacity--)
            {
                var t = Q.Dequeue();

                if (t is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                        
                Object.Destroy(t);
            }
        }

        public override T? Get()
        {
            if (PoolingCount == 0)
            {
                return null;
            }

            T t = Q.Dequeue();
            t.enabled = true;
            
            if (t is IObjectPoolable poolable)
            {
                poolable.OnGet();
            }

            return t;
        }
        
        public override void Return(T t)
        {
            if (t is IObjectPoolable poolable)
            {
                poolable.OnReturn();
            }

            t.enabled = false;
            
            Q.Enqueue(t);
        }
    }
    
    /// <summary>
    /// unity의 GameObject에 대한 ObjectPool입니다.
    /// Prefab에 쓰세요
    /// </summary>
    [Serializable]
    public class GameObjectPool : ObjectPool<PooledObject>
    {
        public static GameObjectPool GetOrCreate(GameObject prefab, int initialSize = -1)
        {
            if (PoolDict.TryGetValue(prefab.name.GetHashCode(), out ObjectPool pool))
            {
                return (GameObjectPool)pool;
            }
            
            var newPool = new GameObjectPool(prefab, ObjectPoolMonoBehaviour.GameObject);
            newPool._initialCapacity = initialSize < 0 ? DEFAULT_CAPACITY : initialSize;
            TryAddPool(newPool.HashKey, newPool);
            newPool.initialize(newPool._initialCapacity);
            
            return newPool;
        }
        
        public string Key => _prefab.name;
        public int HashKey => Key.GetHashCode();
        
        [SerializeField]
        private GameObject _prefab;
        [SerializeField]
        private int _initialCapacity;
        
        [field:SerializeField]
        public GameObject Parent { get; private set; }
        public GameObject? gameObject { get; private set; }

        private GameObjectPool(GameObject prefab, GameObject parentGameObject)
        {
            _prefab = prefab;
            Parent = parentGameObject;
        }

        public void Init()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            Q = new Queue<PooledObject>();
            instantiate(_initialCapacity);
        }

        protected override void instantiate(int number)
        {
            if (Parent == null)
            {
                Parent = UnityObjectPool.GetOrCreate().gameObject;
            }
            
            if (gameObject == null)
            {
                gameObject = new GameObject($"{Key} ObjectPool");
                gameObject.transform.SetParent(Parent.transform);
            }

            _prefab.SetActive(false);
            for (int i = 0; i < number; i++, Capacity++)
            {
                PooledObject obj = Object.Instantiate(_prefab, Vector3.zero, Quaternion.identity, gameObject.transform).AddComponent<PooledObject>();

                obj.gameObject.name = $"{Key}_PooledObject {Capacity}";

                Q.Enqueue(obj);
            }
            _prefab.SetActive(true);
        }

        protected override void destroy(int number)
        {
            for (int i = 0; i < number && PoolingCount > 0; i++, Capacity--)
            {
                var t = Q.Dequeue();
                        
                Object.Destroy(t);
            }
        }
        
        public override PooledObject? Get()
        {
            if (PoolingCount == 0)
            {
                return null;
            }

            PooledObject t = Q.Dequeue();
            t.gameObject.SetActive(true);
            
            return t;
        }
        
        public override void Return(PooledObject t)
        {
            t.gameObject.SetActive(false);
            
            Q.Enqueue(t);
        }
    }
    
    /// <summary>
    /// unity의 특정 컴포넌트를 GameObject와 함께 쓰고 싶을때 사용합니다.
    /// </summary>
    [Serializable]
    public class GameObjectPool<T> : ObjectPool<T> where T : Behaviour
    {
        public static GameObjectPool<T> GetOrCreate(Transform parent, int initialSize = -1)
        {
            if (PoolDict.TryGetValue((typeof(T).FullName + "Pool" + parent.GetHashCode()).GetHashCode(), out ObjectPool pool))
            {
                return (GameObjectPool<T>)pool;
            }
            
            var newPool = new GameObjectPool<T>(ObjectPoolMonoBehaviour.GameObject.transform);
            newPool._initialCapacity = initialSize < 0 ? DEFAULT_CAPACITY : initialSize;
            TryAddPool(newPool.HashKey, newPool);
            newPool.initialize(newPool._initialCapacity);
            
            return newPool;
        }
        
        public string Key => typeof(T).FullName + "Pool" + Parent.GetHashCode();
        public int HashKey => Key.GetHashCode();

        [SerializeField]
        private int _initialCapacity;
        
        [field:SerializeField]
        public Transform Parent { get; private set; }

        private GameObjectPool (Transform parentGameObject)
        {
            Parent = parentGameObject;
        }

        public void Init(Action<T>? initWith = null)
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            Q = new Queue<T>();
            instantiate(_initialCapacity);
            if (initWith != null)
            {
                foreach (var t in Q)
                {
                    initWith(t);
                }
            }
        }

        protected override void instantiate(int number)
        {
            if (Parent == null)
            {
                Parent = UnityObjectPool.GetOrCreate().gameObject.transform;
            }

            for (int i = 0; i < number; i++, Capacity++)
            {
                GameObject gameObject = new GameObject($"{typeof(T)}_PooledObject");
                gameObject.transform.SetParent(Parent);
                var t = gameObject.AddComponent<T>();
                
                t.enabled = false;
                
                Q.Enqueue(t);
            }
        }

        protected override void destroy(int number)
        {
            for (int i = 0; i < number && PoolingCount > 0; i++, Capacity--)
            {
                var t = Q.Dequeue();
                        
                Object.Destroy(t);
            }
        }
        
        public override T? Get()
        {
            if (PoolingCount == 0)
            {
                return null;
            }

            T t = Q.Dequeue();
            t.enabled = true;
            
            return t;
        }
        
        public override void Return(T t)
        {
            t.enabled = false;
            
            Q.Enqueue(t);
        }
    }
}