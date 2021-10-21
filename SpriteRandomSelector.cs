using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class SpriteRandomSelector : MonoBehaviour
    {
        public List<Sprite> SpriteList;
        public SpriteRenderer SpriteRenderer;

        void OnEnable()
        {
            SpriteRenderer.sprite = SpriteList[Random.Range(0, SpriteList.Count)];
        }
    }
}