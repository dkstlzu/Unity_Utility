using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace dkstlzu.Utility
{
    public class ResourcesExtension
    {
        public static string ResourcesPath = Application.dataPath + "/Resources";

        /// <summary>
        /// This method does not function on build.
        /// </summary>
        public static UnityEngine.Object LoadSubDirectory (string resourceName, System.Type systemTypeInstance) 
        {
            return LoadSubDirectory(string.Empty, resourceName, systemTypeInstance);
        }

        /// <summary>
        /// This method does not funciton on Build.
        /// </summary>
        public static UnityEngine.Object LoadSubDirectory (string subDirectory, string resourceName, System.Type systemTypeInstance)
        {
            string[] directories = Directory.GetDirectories(Path.Combine(ResourcesPath, subDirectory), "*", SearchOption.AllDirectories);
            List<string> directoriesList = new List<string>(directories);
            return LoadSubDirectory(directoriesList, resourceName, systemTypeInstance);
        }

        public static UnityEngine.Object LoadSubDirectory(List<string> subDirectories, string resourceName, System.Type systemTypeInstance)
        {
            foreach (var item in subDirectories)
            {
                string itemPath = item.Substring(ResourcesPath.Length + 1);
                UnityEngine.Object result = Resources.Load(itemPath + "/" + resourceName, systemTypeInstance);
                if(result != null)
                    return result;
            }
            return null;
        }

        public static UnityEngine.Object LoadSubDirectory<T> (string resourceName) where T : UnityEngine.Object
        {
            return LoadSubDirectory<T> (string.Empty, resourceName);
        }

        public static UnityEngine.Object LoadSubDirectory<T> (string subDirectory, string resourceName) where T : UnityEngine.Object
        {
            string[] directories = Directory.GetDirectories(Path.Combine(ResourcesPath, subDirectory), "*", SearchOption.AllDirectories);
            List<string> subDirectories = new List<string>(directories);
            return LoadSubDirectory<T>(subDirectories, resourceName);
        }

        public static UnityEngine.Object LoadSubDirectory<T> (List<string> subDirectories, string resourceName) where T : UnityEngine.Object
        {
            foreach (var item in subDirectories)
            {
                string itemPath = item.Substring(ResourcesPath.Length + 1);
                UnityEngine.Object result = Resources.Load<T>(itemPath + "\\" + resourceName);
                if(result != null)
                    return result;
            }
            return null;
        }
    }
}