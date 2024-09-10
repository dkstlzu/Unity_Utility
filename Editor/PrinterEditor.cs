using UnityEditor;

namespace dkstlzu.Utility
{
    public static class PrinterEditor
    {
        [MenuItem("Dev/Printer/Up Priority %PGUP", false, 1)]
        public static void UpPriority()
        {
            Printer.CurrentPriority++;
            Notify();
        }

        [MenuItem("Dev/Printer/Down Priority %PGDN", false, 2)]
        public static void DownPriority()
        {
            Printer.CurrentPriority--;
            Notify();
        }
        
        [MenuItem("Dev/Printer/Print individual %#PGUP", false, 3)]
        public static void DefulatIntervalAsIndividual()
        {
            Printer.DefaultLogInterval = LogInterval.Individual;
            Notify();
        }

        [MenuItem("Dev/Printer/Print frame %#PGDN", false, 4)]
        public static void DefulatIntervalAsFrame()
        {
            Printer.DefaultLogInterval = LogInterval.Frame;
            Notify();
        }

        static void Notify()
        {
            Printer.Print($"Printer update. Priority : {Printer.CurrentPriority}, Interval : {Printer.DefaultLogInterval}", customTag:"Dev", priority:Printer.CurrentPriority, logInterval:LogInterval.Individual);
        }
    }
}