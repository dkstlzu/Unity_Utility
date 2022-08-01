using UnityEngine;

namespace dkstlzu.Utility
{
    public class DontDestroyManager : MonoBehaviour
    {
        public Component TargetComponent;
        public bool useUniqueComponent;

        public UniqueComponent UniqueComponent;

        void Awake()
        {
            DontDestroyOnLoad(TargetComponent.transform.root.gameObject);
        }

        void OnValidate()
        {
            if (useUniqueComponent)
                SynchronizeWithUniqueComponent();
        }

        public void SynchronizeWithUniqueComponent()
        {
            UniqueComponent.TargetComponent = TargetComponent;
        }
    }
}