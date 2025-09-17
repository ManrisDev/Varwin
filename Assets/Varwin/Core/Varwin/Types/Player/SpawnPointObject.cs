using UnityEngine;

namespace Varwin
{
    public class SpawnPointObject : MonoBehaviour
    {
        public static SpawnPointObject Instance;
        public Transform SpawnPoint;

        private void Awake()
        {
            Instance = this;
        }
    }
}