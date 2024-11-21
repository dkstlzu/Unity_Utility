using System.IO;
using UnityEngine;

namespace dkstlzu.Utility
{
    public static class Assets
    {
        public static void CheckDirectory(string path)
        {
            string fullPath = Path.Combine(Application.dataPath.Replace("/Assets", ""), path);
            
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        public static void CheckFilePath(string path)
        {
            string fullPath = Path.Combine(Application.dataPath.Replace("/Assets", ""), path);
            string directoryPath = Path.GetDirectoryName(fullPath);
            
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
    }
}