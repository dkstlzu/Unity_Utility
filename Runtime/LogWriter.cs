using System.IO;
using System;
using UnityEngine;

namespace dkstlzu.Utility
{
    public class LogWriter
    {
        public string FolderPath;
        public string FileName;
        public string Extention;

        public string FullPath => Path.Combine(Application.streamingAssetsPath, FolderPath) + FileName + "." + Extention;
        public bool IsValid => _writer != null;

        protected StreamWriter _writer = null;

        public LogWriter() {}
        public LogWriter(string folderPath, string fileName, string extention)
        {
            FolderPath = folderPath;
            FileName = fileName;
            Extention = extention;

            Directory.CreateDirectory(System.IO.Path.Combine(Application.streamingAssetsPath, FolderPath));

            try
            {

                if (!File.Exists(FullPath)) 
                {
                    _writer = new StreamWriter(File.Create(FullPath));
                } else
                {
                    _writer = new StreamWriter(FullPath, true);
                }
                _writer.AutoFlush = true;

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
            _writer.Write(content);
            if (newLine) NewLine();
        }

        public void NewLine()
        {
            _writer.Write("\n");
        }

        public void Flush()
        {
            _writer.Flush();
        }

        public virtual void Close()
        {
            _writer.Close();
        }

        public void DebugWrite(string content)
        {
            _writer.Write("Debug msg : " + content);
            _writer.WriteLine();
        }
    }
}