using System.Collections.Generic;

namespace Varwin
{
    public static class CollectionUtils
    {
        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> collection)
        {
            foreach (T element in collection)
            {
                hashSet.Add(element);
            }
        }
    }
}