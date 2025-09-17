using System;

namespace Varwin
{
    public sealed class ObjectSpawnedEventArgs : EventArgs
    {
        public ObjectController ObjectController;
        public bool InternalSpawn;
        public bool Duplicated;

        /// <summary>
        /// Причиной для спавна стало то, что объект находится в иерархии родительсякого объекта
        /// </summary>
        public bool SpawnedByHierarchy { get; set; }
    }
}