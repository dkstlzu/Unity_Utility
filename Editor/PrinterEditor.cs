using UnityEngine;

namespace dkstlzu.Utility
{
    public class PrinterEditor
    {
        [UnityEditor.MenuItem("DevTest/Printer Switch ^#&p")]
        public static void SwitchPrinter()
        {
            Printer.Use = !Printer.Use;
            Debug.Log($"Printer On : {Printer.Use}");
        }

        [UnityEditor.MenuItem("DevTest/DebugPrinter Switch ^#&o")]
        public static void SwitchPrinterDebug()
        {
            Printer.UseDebugPrint = !Printer.UseDebugPrint;
            Debug.Log($"DebugPrinter On : {Printer.UseDebugPrint}");
        }
    }
}