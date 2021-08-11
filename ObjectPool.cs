using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    [System.Serializable]
    public class PoolSettings
    {
        public PoolType PoolName;
        public string SourceFilePath = "";
        public int MaxPoolSize = 10;
        public bool AutoReturn = false;
        public float AutoReturnTime = 5;

    }

    public enum PoolType
    {
        Docsa,
        Hunter,
        Weapon,
        StarRain,
        Net,
    }

    /// <summary>
    /// Pooling
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        public static Dictionary<PoolType, ObjectPool> SPoolDict = new Dictionary<PoolType, ObjectPool>();
        [Header("Pool Object Settings")]
        public PoolType PoolName;

        public GameObject SourceObject;
        public string SoruceFilePath;

        [Header("Capacity Settings")]
        public int MaxPoolSize = 5;
        public bool AutoReturn;
        public float AutoReturnTime;

        public GameObject[] ActiveObjects
        {
            get 
            {
                GameObject[] objs = new GameObject[ActiveObjectList.Count];
                ActiveObjectList.CopyTo(objs);
                return objs;
            }
        }
        private Queue<GameObject> AvailableObjectQueue = new Queue<GameObject>();
        private List<GameObject> ActiveObjectList = new List<GameObject>();

        public static ObjectPool GetOrCreate(PoolType poolName, GameObject OnCallerGameObject = null)
        {
            ObjectPool pool;
            if (!SPoolDict.TryGetValue(poolName, out pool))
            {
                if (OnCallerGameObject == null)
                {
                    pool = new GameObject(poolName + "ObjectPool").AddComponent<ObjectPool>();
                } else
                {
                    pool = OnCallerGameObject.AddComponent<ObjectPool>();
                }
                SPoolDict.Add(poolName, pool);
            }

            return pool;
        }
        // public static ObjectPool GetOrCreate(string poolName, GameObject OnCallerGameObject = null)
        // {
        //     ObjectPool pool;
        //     if (!SPoolDict.TryGetValue(poolName, out pool))
        //     {
        //         if (OnCallerGameObject == null)
        //         {
        //             pool = new GameObject(poolName + "ObjectPool").AddComponent<ObjectPool>();
        //         } else
        //         {
        //             pool = OnCallerGameObject.AddComponent<ObjectPool>();
        //         }
        //         SPoolDict.Add(poolName, pool);
        //     }

        //     return pool;
        // }

        public ObjectPool Init(PoolSettings poolSettings)
        {
            // Initialize PoolSettings
            PoolName = poolSettings.PoolName;
            SoruceFilePath = poolSettings.SourceFilePath;
            MaxPoolSize = poolSettings.MaxPoolSize;
            AutoReturn = poolSettings.AutoReturn;
            AutoReturnTime = poolSettings.AutoReturnTime;

            if(SourceObject == null)
            {
                SourceObject = Resources.Load(SoruceFilePath, typeof(GameObject)) as GameObject;
            }

            Allocate();

            return this;
        }

        private void Allocate()
        {
            for (int i =0 ; i<MaxPoolSize; i++)
            {
                GameObject pooledObject = Instantiate(SourceObject, transform);

                pooledObject.name = pooledObject.name + i.ToString();
                AvailableObjectQueue.Enqueue(pooledObject);
                pooledObject.SetActive(false);
            }
        }

        /// <summary>
        /// ObjectPool에서 Creature를 가져옵니다.
        /// </summary>
        /// <param name="pos">위치</param>
        /// <param name="rot">회전</param>
        /// <returns>해당 Pool이 가지고 있는 Creature Script</returns>
        public GameObject Instantiate(Vector3 pos, Quaternion rot)
        {
            GameObject obj;

            int AvailableCount = AvailableObjectQueue.Count;

            if (AvailableCount <= 0)
            {
                return null;
            }else
            {
                obj = AvailableObjectQueue.Dequeue();
                ActiveObjectList.Add(obj);
                obj.transform.SetPositionAndRotation(pos, rot);
                obj.SetActive(true);
            }

            if (AutoReturn)
            {
                StartCoroutine(AutoReturnCoroutine(obj));
            }

            return obj;
        }



        /// <summary>
        /// ObjectPool에서 Creature를 가져옵니다. 그러나 OnEnable이 불리기 전에 Initiater를 통해 초기화작업을 수행합니다.
        /// </summary>
        /// <param name="pos">위치</param>
        /// <param name="rot">회전</param>
        /// <returns>해당 Pool이 가지고 있는 Creature Script</returns>
        public GameObject InstantiateAfterInit(Vector3 pos, Quaternion rot, NetInitiater initiater)
        {
            GameObject obj;

            int AvailableCount = AvailableObjectQueue.Count;

            if (AvailableCount <= 0)
            {
                return null;
            }else
            {
                obj = AvailableObjectQueue.Dequeue();
                initiater(obj.GetComponent<Docsa.ProjectileNet>());
                ActiveObjectList.Add(obj);
                obj.transform.SetPositionAndRotation(pos, rot);
                obj.SetActive(true);
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
                obj.SetActive(false);
                ActiveObjectList.Remove(obj);
                AvailableObjectQueue.Enqueue(obj);
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
        /// Pool을 Dispose합니다.
        /// Game이 끝났을때 이용합니다.
        /// </summary>
        public void Dispose()
        {
            ReturnAll();

            while(AvailableObjectQueue.Count > 0)
            {
                GameObject obj = AvailableObjectQueue.Dequeue();
                Destroy(obj.gameObject);
            }
            AvailableObjectQueue = null;
            ActiveObjectList = null;
            SPoolDict.Remove(PoolName);

            Destroy(gameObject);
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