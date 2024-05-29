using System.Reflection;

namespace dkstlzu.Utility
{
    public static class UnityConsole
    {
        public static void ClearConsole()
        {
            var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method!.Invoke(new object(), null);
        }
    }
}