using System.IO;
using System;
using UnityEngine;
using System.Threading.Tasks;

namespace dkstlzu.Utility
{
    public class LogWriter
    {
        public string Path;
        public string FileName;
        public string Extention;
        public string FullPath {get {return System.IO.Path.Combine(Application.streamingAssetsPath, Path) + FileName + "." + Extention;}}
        protected StreamWriter writer = null;

        public bool isValid
        {
            get {return writer != null;}
        }

        public string testStr;

        public LogWriter() {}
        public LogWriter(string path, string fileName, string extention)
        {
            Path = path;
            FileName = fileName;
            Extention = extention;

            Directory.CreateDirectory(System.IO.Path.Combine(Application.streamingAssetsPath, Path));

            try
            {

                if (!File.Exists(FullPath)) 
                {
                    writer = new StreamWriter(File.Create(FullPath));
                } else
                {
                    writer = new StreamWriter(FullPath, true);
                }
                writer.AutoFlush = true;

                string recordingStartMessage = $"-----------------------------------\nRecording start {DateTime.Now}.";

                Write(recordingStartMessage, true);
                NewLine();
                Write("Recorded Contents : ", true);
            } catch (Exception e)
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
            writer.Write(content);
            if (newLine) NewLine();
        }

        public void NewLine()
        {
            writer.Write("\n");
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