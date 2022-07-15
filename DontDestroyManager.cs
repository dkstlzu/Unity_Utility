using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    [RequireComponent(typeof(UniqueComponent))]
    public class DontDestroyManager : MonoBehaviour
    {
        [SerializeField] private Component _targetComponent;
        public Component TargetComponent
        {
            get
            {
                return _targetComponent;
            }

            set
            {
                _targetComponent = value;
                SynchronizeWithUniqueComponent();
            }
        }

        [SerializeField] private UniqueComponent _uniqueComponent;

        void Reset()
        {
            _uniqueComponent = GetComponent<UniqueComponent>();
        }

        void Awake()
        {
            DontDestroyOnLoad(_targetComponent.transform.root.gameObject);
        }

        void OnValidate()
        {
            SynchronizeWithUniqueComponent();
        }

        public void SynchronizeWithUniqueComponent()
        {
            _uniqueComponent.TargetComponent = _targetComponent;
        }
    }
}