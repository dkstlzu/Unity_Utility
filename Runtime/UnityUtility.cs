using UnityEngine;

namespace dkstlzu.Utility
{
    public static class UnityUtility
    {
        public static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

}