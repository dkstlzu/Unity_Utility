using UnityEngine;

namespace dkstlzu.Utility
{
    public static class Printer
    {
        public static bool Use;
        public static bool UseDebugPrint;

        public static void DebugPrint(object message)
        {
            if (UseDebugPrint) MonoBehaviour.print(message);
        }
        
        public static void Print(object message)
        {
            if (Use) MonoBehaviour.print(message);
        }

        public static void Log(object message)
        {
            if (Use) Debug.Log(message);
        }

        public static void LogWarning(object message)
        {
            if (Use) Debug.LogWarning(message);
        }

        public static void LogError(object message)
        {
            if (Use) Debug.LogError(message);
        }

#if Unity_Editor
        [UnityEditor.MenuItem("DevTest/Printer Switch ^#&p")]
        public static void SwitchPrinter()
        {
            Use = !Use;
            Debug.Log($"Printer On : {Use}");
        }

        [UnityEditor.MenuItem("DevTest/DebugPrinter Switch ^#&o")]
        public static void SwitchPrinterDebug()
        {
            UseDebugPrint = !UseDebugPrint;
            Debug.Log($"DebugPrinter On : {UseDebugPrint}");
        }
#endif
    }
}