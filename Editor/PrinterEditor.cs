#if DEBUG
#define LOG_ON
#endif

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
        public static void DefaultIntervalAsIndividual()
        {
            Printer.DefaultLogInterval = LogInterval.Individual;
            Notify();
        }
    
        [MenuItem("Dev/Printer/Print individual %#PGUP", true, 3)]
        public static bool DefaultIntervalAsIndividual_Validation()
        {
            return Printer.DefaultLogInterval != LogInterval.Individual;
        }

        [MenuItem("Dev/Printer/Print frame %#PGDN", false, 4)]
        public static void DefaultIntervalAsFrame()
        {
            Printer.DefaultLogInterval = LogInterval.Frame;
            Notify();
        }

        [MenuItem("Dev/Printer/Print frame %#PGDN", true, 4)]
        public static bool DefaultIntervalAsFrame_Validation()
        {
            return Printer.DefaultLogInterval != LogInterval.Frame;
        }
    
        static void Notify()
        {
            Printer.Print($"Printer update. Priority : {Printer.CurrentPriority}, Interval : {Printer.DefaultLogInterval}", customTag:"Dev", priority:Printer.CurrentPriority, logInterval:LogInterval.Individual);
        }
    }
}