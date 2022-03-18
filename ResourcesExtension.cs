using UnityEngine;
using System.IO;
using System;

public class ResourcesExtension
{
    public static string ResourcesPath = Application.dataPath + "/Resources";

    public static UnityEngine.Object LoadSubDirectory (string resourceName, System.Type systemTypeInstance) 
    {
        return LoadSubDirectory(string.Empty, resourceName, systemTypeInstance);
    }

    public static UnityEngine.Object LoadSubDirectory (string subDirectory, string resourceName, System.Type systemTypeInstance)
    {
        string[] directories = Directory.GetDirectories(Path.Combine(ResourcesPath, subDirectory), "*", SearchOption.AllDirectories);
        foreach (var item in directories)
        {
            string itemPath = item.Substring(ResourcesPath.Length + 1);
            UnityEngine.Object result = Resources.Load(itemPath + "\\" + resourceName, systemTypeInstance);
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
        foreach (var item in directories)
        {
            string itemPath = item.Substring(ResourcesPath.Length + 1);
            UnityEngine.Object result = Resources.Load<T>(itemPath + "\\" + resourceName);
            if(result != null)
                return result;
        }
        return null;
    }
}