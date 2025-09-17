using System.IO;
using UnityEngine;

namespace Varwin.Editor
{
    public static class FileInfoEx
    {
        public static string GetAssetPath(this FileInfo fileInfo)
        {
            return fileInfo?.FullName?.Replace('\\', '/').Replace(UnityProject.Path, "").TrimStart('/');
        }
    }
}