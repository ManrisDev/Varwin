using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Assimp
{
    public class AssimpImporter
    {
        public static AssimpModelContainer Load(string modelPath)
        {
            throw new System.NotImplementedException();
        }
        
        public static async Task<AssimpModelContainer> LoadAsync(string filePath, Func<Task> skipFrameTask, Transform parent = null)
        {
            throw new System.NotImplementedException();
        }
    }

    public class AssimpModelContainer : MonoBehaviour
    {
        public GameObject gameObject;

        public void Unload()
        {
            
        }
        
        public static void UnloadAll() { }
    }
}