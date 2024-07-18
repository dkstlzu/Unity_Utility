using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace dkstlzu.Utility
{
    internal class WindowPrinterMonoBehaviour : Singleton<WindowPrinterMonoBehaviour>
    {
        struct WindowPrinterTimer
        {
            public string Message;
            public float InvokedTime;
        }
        
        public Rect Box = new Rect(50, 50, 500, 1000);
        public float PrintDuration = 3f;
        private Queue<WindowPrinterTimer> msgs = new Queue<WindowPrinterTimer>();

        public void Enqueue(string str)
        {
            msgs.Enqueue(new WindowPrinterTimer()
            {
                Message = str,
                InvokedTime = Time.realtimeSinceStartup
            });
        }
        
        private void OnGUI()
        {
            StringBuilder stringBuilder = new StringBuilder();
                
            var enumerator = msgs.GetEnumerator();
            while (enumerator.MoveNext())
            {
                stringBuilder.AppendLine(enumerator.Current.Message);
            }

            GUI.skin.label.fontSize = 20;
            GUI.Label(Box, stringBuilder.ToString());
        }

        private void Update()
        {
            if (msgs.TryPeek(out WindowPrinterTimer timer))
            {
                if (timer.InvokedTime + PrintDuration < Time.realtimeSinceStartup)
                {
                    msgs.Dequeue();
                }
            }
        }
    }
}