using System.IO;
using UnityEngine;

namespace Varwin.Editor
{
    public static class DirectoryInfoEx
    {
        public static string GetAssetPath(this DirectoryInfo directoryInfo)
        {
            return directoryInfo.FullName.Replace('\\', '/').Replace(UnityProject.Path, "").TrimStart('/');
        }
    }
}