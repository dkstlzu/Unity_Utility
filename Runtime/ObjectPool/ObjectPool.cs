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
        private bool _initialized = false;

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
    public class ClassObjectPool<T> : ObjectPool<T> where T : class, new()
    {
        private static string Key => typeof(T).Name;
        private static int HashKey => Key.GetHashCode();
        private static ClassObjectPool<T>? _instance;
        
        public static ClassObjectPool<T> GetOrCreate(int initialSize = -1)
        {
            if (_instance != null)
            {
                return _instance;
            }
            
            if (!ContainsPool(HashKey))
            {
                if (TryAddPool(HashKey, new ClassObjectPool<T>()))
                {
                    ((ClassObjectPool<T>)PoolDict[HashKey]).initialize(initialSize < 0 ? DEFAULT_CAPACITY : initialSize);
                }
            }
            
            _instance = (ClassObjectPool<T>)PoolDict[HashKey];
        
            return _instance;
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
            for (int i = 0; i < number && Q.Count > 0; i++, Capacity--)
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
            if (Q.Count == 0)
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
    public class BehaviourObjectPool<T> : ObjectPool<T> where T : Behaviour
    {
        private static string Key => typeof(T).Name;
        private static int HashKey => Key.GetHashCode();
        private static BehaviourObjectPool<T>? _instance;
        
        public static BehaviourObjectPool<T> GetOrCreate(int initialSize = -1)
        {
            if (_instance != null)
            {
                return _instance;
            }
            
            if (!ContainsPool(HashKey))
            {
                if (TryAddPool(HashKey, new BehaviourObjectPool<T>(ObjectPoolMonoBehaviour.GameObject)))
                {
                    ((BehaviourObjectPool<T>)PoolDict[HashKey]).initialize(initialSize < 0 ? DEFAULT_CAPACITY : initialSize);
                }
            }
            
            _instance = (BehaviourObjectPool<T>)PoolDict[HashKey];
        
            return _instance;
        }

        public GameObject Parent { get; }
        [field:SerializeField]
        public GameObject? gameObject { get; private set; }

        [SerializeField] private int _initialCapacity;
        
        private BehaviourObjectPool(GameObject parentGameObject)
        {
            Parent = parentGameObject;
        }

        public void Init()
        {
            instantiate(_initialCapacity);
        }

        protected override void instantiate(int number)
        {
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
            for (int i = 0; i < number && Q.Count > 0; i++, Capacity--)
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
            if (Q.Count == 0) return null;

            T t = Q.Dequeue();
            t.gameObject.SetActive(true);
            
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
            
            t.gameObject.SetActive(false);
            
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
            if (!ContainsPool(prefab.name))
            {
                if (TryAddPool(prefab.name, new GameObjectPool(prefab, ObjectPoolMonoBehaviour.GameObject)))
                {
                    ((GameObjectPool)PoolDict[prefab.name.GetHashCode()]).initialize(initialSize < 0 ? DEFAULT_CAPACITY : initialSize);
                }
            }
            
            return (GameObjectPool)PoolDict[prefab.name.GetHashCode()];
        }
        
        [SerializeField]
        private GameObject _prefab;
        [SerializeField]
        private int _initialCapacity;
        
        public string Key => _prefab.name;
        public int HashKey => _prefab.GetHashCode();
        
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
            if (Q == null)
            {
                Q = new Queue<PooledObject>();
            }
            instantiate(_initialCapacity);
        }

        protected override void instantiate(int number)
        {
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
            for (int i = 0; i < number && Q.Count > 0; i++, Capacity--)
            {
                var t = Q.Dequeue();
                        
                Object.Destroy(t);
            }
        }
        
        public override PooledObject? Get()
        {
            if (Q.Count == 0) return null;

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
}