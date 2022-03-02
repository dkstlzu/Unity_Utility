using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    /// <summary>
    /// Pooling
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        // For Editor Script
        [SerializeField] private string EnumName;
        [SerializeField] private string EnumValue;
        public bool EnumNameCorrect;
        public List<string> IncludedEnumStringList = new List<string>();
        public List<int> IncludedEnumCountList = new List<int>();

        public static void AddEnumCount(Enum enumValue, ObjectPool pool)
        {
            if (SPoolTupleDict.ContainsKey(enumValue))
            {
                SPoolTupleDict[enumValue] = (SPoolTupleDict[enumValue].Pool, SPoolTupleDict[enumValue].Count + 1);
            } else
            {
                SPoolTupleDict.Add(enumValue, (pool, 1));
            }
        }

        public static void SubtractEnumCount(Enum enumValue)
        {
            if (SPoolTupleDict.ContainsKey(enumValue))
            {
                SPoolTupleDict[enumValue] = (SPoolTupleDict[enumValue].Pool, SPoolTupleDict[enumValue].Count - 1);
                if (SPoolTupleDict[enumValue].Item2 <= 0)
                {
                    SPoolTupleDict.Remove(enumValue);
                }
            } else
            {
                Debug.LogWarning($"{enumValue} is not in static ObjectPool.SPoolTupleDict.");
            }
        }
        // Static PoolDict
        public static Dictionary<Enum, (ObjectPool Pool, int Count)> SPoolTupleDict = new Dictionary<Enum, (ObjectPool, int)>();
        // PoolSettings
        public Enum PoolEnum
        {
            get
            {
                Enum val;
                try
                {
                    Type enumType = EnumHelper.GetEnumType(EnumName);
                    if (!Enum.IsDefined(enumType, EnumValue))
                        EnumValue = ((Enum)Enum.GetValues(enumType).GetValue(0)).ToString();
                    val = Enum.Parse(enumType, EnumValue) as Enum;
                } catch
                {
                    Debug.LogWarning($"ObjectPool({gameObject.name}) EnumName or EnumValue is wrong. Check again.");
                    return null;
                }
                return val;
            }
        }
        public UnityEngine.GameObject SourceObject = null;
        public string SourceFilePath = string.Empty;
        public int PoolSize = 10;
        public int Count
        {
            get {return ActiveObjectList.Count + AvailableObjectList.Count;}
        }
        public bool AutoReturn = false;
        public float AutoReturnTime = 5;
        //

        public GameObject[] ActiveObjects{get{return ActiveObjectList.ToArray();}}
        public GameObject[] AvailableObjects{get{return AvailableObjectList.ToArray();}}
        public bool isAllocated;

        [SerializeField] private List<GameObject> ActiveObjectList = new List<GameObject>();
        [SerializeField] private List<GameObject> AvailableObjectList = new List<GameObject>();

        void Start()
        {
            ObjectPool.AddEnumCount(PoolEnum, this);
        }

        public static ObjectPool GetOrCreate(Enum poolName, GameObject poolGameObject = null)
        {
            
            (ObjectPool, int) tuple;
            if (!SPoolTupleDict.TryGetValue(poolName, out tuple))
            {
                if (poolGameObject == null)
                {
                    tuple = (new GameObject(poolName + "ObjectPool").AddComponent<ObjectPool>(), 1);
                } else
                {
                    tuple = (poolGameObject.AddComponent<ObjectPool>(), 1);
                }
                SPoolTupleDict.Add(poolName, tuple);
            }

            return tuple.Item1;
        }

        public static ObjectPool GetOrCreate(string poolNameString, GameObject poolGameObject = null)
        {
            Type type = null;
            foreach(var v in SPoolTupleDict)
            {
                if (Enum.IsDefined(v.Key.GetType(), poolNameString))
                {
                    type = v.Key.GetType();
                }
            }

            if (type == null)
            {
                Debug.LogWarning("ObjectPool.GetOrCreate(string poolNameString) : Wrong PoolNameString");
                return null;
            }

            Enum poolName = (Enum)Enum.Parse(type, poolNameString);
            return GetOrCreate(poolName, poolGameObject);
        }

        public void Allocate()
        {
            if (SourceObject == null)
            {
                Debug.LogError(gameObject.name + " ObjectPool SourceObject is null. Check again");
                return;
            }

            for (int i =0 ; i<PoolSize; i++)
            {
                GameObject pooledObject = Instantiate(SourceObject as GameObject, transform);

                pooledObject.name = pooledObject.name + i.ToString();
                AvailableObjectList.Add(pooledObject);
                pooledObject.SetActive(false);
            }
            isAllocated = true;
        }

        public delegate void Initiater(UnityEngine.GameObject targetObject);

        /// <summary>
        /// ObjectPool에서 Creature를 가져옵니다.
        /// OnEnable이 불리기 전에 PreInitiater를 통해 초기화작업을 수행합니다.
        /// OnEnable이 불린후 PostInitiater를 통해 초기화작업을 수행합니다.
        /// </summary>
        /// <param name="pos">위치</param>
        /// <param name="rot">회전</param>
        /// <returns>해당 Pool이 가지고 있는 Creature Script</returns>
        public GameObject Instantiate(Vector3 pos, Quaternion rot, Initiater preInitiater = null, Initiater postInitiater = null)
        {
            GameObject obj;

            int AvailableCount = AvailableObjectList.Count;

            if (AvailableCount <= 0)
            {
                return null;
            }else
            {
                obj = AvailableObjectList[0];
                AvailableObjectList.RemoveAt(0);

                // Initiater
                if (preInitiater != null)
                    preInitiater(obj);

                ActiveObjectList.Add(obj);
                obj.transform.SetPositionAndRotation(pos, rot);
                obj.SetActive(true);

                if (postInitiater != null)
                    postInitiater(obj);
            }

            if (AutoReturn)
            {
                StartCoroutine(AutoReturnCoroutine(obj));
            }

            return obj;
        }

        /// <summary>
        /// Creature를 ObjectPool에 반환합니다.
        /// </summary>
        /// <param name="obj">반환하고자 하는 Creature Script</param>
        public void Return(GameObject obj)
        {
            if (ActiveObjectList.Contains(obj))
            {
                obj.transform.SetParent(transform);
                obj.SetActive(false);
                ActiveObjectList.Remove(obj);
                AvailableObjectList.Add(obj);
            }
        }

        public bool Contains(GameObject obj)
        {
            return ActiveObjectList.Contains(obj);
        }

        /// <summary>
        /// 모든 Creature를 반환합니다.
        /// </summary>
        public void ReturnAll()
        {
            GameObject obj;
            int activeNumber = ActiveObjectList.Count;
            for (int i =0; i < activeNumber; i++)
            {
                obj = ActiveObjectList[0];
                Return(obj);
            }
        }

        /// <summary>
        /// Return all of ObjectPool
        /// </summary>
        public static void ReturnAllPools()
        {
            foreach (var pair in SPoolTupleDict)
            {
                pair.Value.Pool.ReturnAll();
            }
        }

        /// <summary>
        /// Pool을 Dispose합니다.
        /// Game이 끝났을때 이용합니다.
        /// </summary>
        public void Dispose()
        {
            ReturnAll();

            while(AvailableObjectList.Count > 0)
            {
                GameObject obj = AvailableObjectList[0];
                AvailableObjectList.RemoveAt(0);
                Destroy(obj.gameObject);
            }
            AvailableObjectList = null;
            ActiveObjectList = null;
            SubtractEnumCount(PoolEnum);

            Destroy(gameObject);
        }

        public void Clear()
        {
            SubtractEnumCount(PoolEnum);
            for (int i = 0; i < AvailableObjectList.Count; i++)
            {
                Destroy(AvailableObjectList[i]);
            }

            for (int i = 0; i < ActiveObjectList.Count; i++)
            {
                Destroy(ActiveObjectList[i]);
            }

            AvailableObjectList.Clear();
            ActiveObjectList.Clear();

            isAllocated = false;
        }

        IEnumerator AutoReturnCoroutine(GameObject obj, float time=-1)
        {
            float realTime;
            if (time < 0)
            {
                realTime = AutoReturnTime;
            } else
            {  
                realTime = time;
            }

            yield return new WaitForSeconds(realTime);

            Return(obj);
        }
    }
}