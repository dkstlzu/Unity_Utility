using UnityEditor;

namespace dkstlzu.Utility
{
    public static class PrinterEditor
    {
        [MenuItem("Dev/Printer/Up Priority %PGUP")]
        public static void UpPriority()
        {
            Printer.CurrentPriority++;
            Notify();
        }

        [MenuItem("Dev/Printer/Down Priority %PGDN")]
        public static void DownPriority()
        {
            Printer.CurrentPriority--;
            Notify();
        }

        static void Notify()
        {
            Printer.Print($"Printer priority update {Printer.CurrentPriority}", customTag:"WF Dev", priority:Printer.CurrentPriority);
        }
    }
}