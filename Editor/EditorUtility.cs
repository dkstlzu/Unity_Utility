using UnityEditor;

namespace dkstlzu.Utility
{
    public static class UnityUtility
    {
        [MenuItem("Dev/도메인 리로드 %#r")]
        public static void ReloadDomain()
        {
            EditorUtility.RequestScriptReload();
        }
    }
}