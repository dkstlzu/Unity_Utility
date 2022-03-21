using System.IO;
using System;
using UnityEngine;
using System.Threading.Tasks;

namespace Utility
{
    public class LogWriter
    {
        public string Path ;
        public string Extention;
        public string FileName {get {return Path + "." + Extention;}}
        static StreamWriter writer = null;

        public LogWriter() {}
        public LogWriter(string path, string extention)
        {
            Path = path;
            Extention = extention;

            try
            {

                if (!File.Exists(FileName)) 
                {
                    File.Create(FileName);
                } 
                writer = new StreamWriter(FileName, true);
                writer.AutoFlush = true;

                string recordingStartMessage = $"Recod start {DateTime.Now}.";

                Write(recordingStartMessage, true);
                NewLine();
                Write("Recorded Contents : ", true);
            }
            catch (Exception e)
            {
                MonoBehaviour.print(e);
            }
        }

        ~LogWriter()
        {
            Flush();
            Close();
        }

        public void Write(string content, bool newLine=false)
        {
            WriteAfterFileExist(content);
            if (newLine) NewLine();
        }

        private async void WriteAfterFileExist(string content)
        {
            if (!File.Exists(FileName))
            {
                File.Create(FileName);
                await Task.Yield();
            }

            writer.Write(content);
        }

        public void NewLine()
        {
            WriteAfterFileExist("\n");
        }

        public void Flush()
        {
            writer.Flush();
        }

        public virtual void Close()
        {
            writer.Close();
        }


        public void DebugWrite(string content)
        {
            writer.Write("Debug msg : " + content);
            writer.WriteLine();
        }
    }
}