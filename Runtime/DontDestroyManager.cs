using UnityEngine;

namespace dkstlzu.Utility
{
    public class DontDestroyManager : MonoBehaviour
    {
        public Component TargetComponent;
        public bool UseUniqueComponent;

        public UniqueComponent UniqueComponent;

        void Awake()
        {
            if (TargetComponent == null)
            {
                Destroy(this);
                return;
            }
            DontDestroyOnLoad(TargetComponent.transform.root.gameObject);
            if (UseUniqueComponent)
            {
                SynchronizeWithUniqueComponent();
            }
        }

        void OnValidate()
        {
            if (UseUniqueComponent)
            {
                SynchronizeWithUniqueComponent();
            }
        }

        public void SynchronizeWithUniqueComponent()
        {
            UniqueComponent.TargetComponent = TargetComponent;
        }
    }
}