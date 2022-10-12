using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dkstlzu.Utility
{
    public class RandomLocator : MonoBehaviour
    {
        public List<Transform> TargetList;
        public RectTransform SpaceRect;

        [ContextMenu("Randomize")]
        public void Randomize()
        {
            foreach(var transform in TargetList)
            {
                transform.position = SpaceRect.position + new Vector3(Random.Range(-SpaceRect.sizeDelta.x/2, SpaceRect.sizeDelta.x/2), Random.Range(-SpaceRect.sizeDelta.y/2, SpaceRect.sizeDelta.y/2), 0);
            }
        }
    }
}