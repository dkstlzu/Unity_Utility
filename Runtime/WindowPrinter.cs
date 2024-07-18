namespace dkstlzu.Utility
{
    public static class WindowPrinter
    {
        private static WindowPrinterMonoBehaviour _instance;

        public static float Duration
        {
            get
            {
                if (_instance == null)
                {
                    _instance = WindowPrinterMonoBehaviour.GetOrCreateDontDestroyOnLoad();
                }
                
                return _instance.PrintDuration;
            }

            set
            {
                if (_instance == null)
                {
                    _instance = WindowPrinterMonoBehaviour.GetOrCreateDontDestroyOnLoad();
                }
                
                _instance.PrintDuration = value;
            }
        }
        
        public static void Print(object message, string customTag = null, int priority = 0)
        {
            if (priority < Printer.CurrentPriority) return;

            string result = Printer.BracketTag(customTag) + message.ToString() + "\n";

            if (_instance == null)
            {
                _instance = WindowPrinterMonoBehaviour.GetOrCreateDontDestroyOnLoad();
            }
            
            _instance.Enqueue(result);
        }
    }
}