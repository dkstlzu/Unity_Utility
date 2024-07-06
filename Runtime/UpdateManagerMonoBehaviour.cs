using UnityEngine;

namespace dkstlzu.Utility
{
    [DefaultExecutionOrder(-10)]
    [AddComponentMenu("UpdateManager")]
    public class UpdateManagerMonoBehaviour : MonoBehaviour
    {
        private UpdateManager _updateManager;
        private FixedUpdateManager _fixedUpdateManager;

#if UNITY_EDITOR
        public int UpdatableNumber;
        public int FixedUpdatableNumber;
#endif

        private void Awake()
        {
            _updateManager = new UpdateManager();
            _fixedUpdateManager = new FixedUpdateManager();
            
            Singleton.RegisterSingleton(_updateManager);
            Singleton.RegisterSingleton(_fixedUpdateManager);
        }

        private void Update()
        {
            _updateManager.Update(default);

#if UNITY_EDITOR
            UpdatableNumber = _updateManager.Count;
            FixedUpdatableNumber = _fixedUpdateManager.Count;
#endif
        }

        private void FixedUpdate()
        {
            _fixedUpdateManager.Update(default);
        }
    }
}