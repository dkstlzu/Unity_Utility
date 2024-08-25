using System;
using UnityEditor;

namespace dkstlzu.Utility
{
    [InitializeOnLoad]
    public static class CustomEditorPlayCallBack
    {
        static CustomEditorPlayCallBack()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public static event Action OnExitingPlayMode;
        public static event Action OnEnteredPlayMode;
        public static event Action OnExitingEditMode;
        public static event Action OnEnteredEditMode;

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                    OnEnteredEditMode?.Invoke();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    OnExitingEditMode?.Invoke();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnEnteredPlayMode?.Invoke();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    OnExitingPlayMode?.Invoke();
                    break;
            }
        }
    }
}
