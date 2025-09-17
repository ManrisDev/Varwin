using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Varwin
{
    public static class FileSystemUtils
    {
        private static string _className = nameof(FileSystemUtils);

        /// <summary>
        /// Safe method to create directory
        /// </summary>
        /// <param name="path">Directory path</param>
        /// <returns>Directory info</returns>
        public static DirectoryInfo CreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            
            string[] folders = path.Split(new[] {'/', '\\'}, StringSplitOptions.RemoveEmptyEntries);
            string prevPath = string.Empty;
            
            foreach (string folder in folders)
            {
                string currPath = prevPath + folder;

                if (!Directory.Exists(currPath))
                {
                    Directory.CreateDirectory(currPath);
                }

                prevPath = currPath + '/';
            }

            return new DirectoryInfo(prevPath);
        }

        public static string GetFilesPath(bool isMobile, string destination)
        {
            string basicPath =  Application.persistentDataPath + "/" + destination;

#if UNITY_EDITOR || !UNITY_ANDROID

            return basicPath;

#endif

            if (!Directory.Exists(basicPath))
            {
                CreateDirectory(basicPath);
            }

            return basicPath;
        }

        public static bool OpenURL(string url)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(url))
            {
                Debug.LogError(_className + ": Failed to open empty URL.");
                return false;
            }

            var lowercase = url.Trim().ToLower();

            if (!(lowercase.StartsWith("http://") || lowercase.StartsWith("https://")))
            {
                Debug.LogError($"{_className}: Failed to open URL {url}.  For security reasons, only HTTP/HTTPS is allowed to open.");
                return false;
            }

            try
            {
                Application.OpenURL(url.Trim());
            }
            catch (Exception e)
            {
                Debug.LogError($"{e} {_className}: Failed to open URL {url}");
                return false;
            }

            return true;
        }

        public static bool OpenFile(string path)
        {
            
#if UNITY_ANDROID
            Debug.LogWarning($"{_className}: Opening files is not available on this platform");
            return false;
#endif
            if (string.IsNullOrWhiteSpace(path))
            {
                Debug.LogError($"{_className}: Failed to open empty path");
                return false;
            }

            try
            {
                path = Path.GetFullPath(path.Replace('/', '\\'));
                if (Directory.Exists(path))
                {//if path is directory
                    var process = new Process();
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = "\"" + path + "\"",
                        WorkingDirectory = path,
                        RedirectStandardError = false,
                        RedirectStandardInput = false,
                        RedirectStandardOutput = false
                    };

                    return process.Start();
                }
                
                if (File.Exists(path))
                {//if path is file
                    Application.OpenURL($"file:///{path}");
                    return true;
                }

                Debug.LogError($"{_className}: File \"{path}\" not found");
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"{e} {_className}: Failed to open file at path \"{path}\"");
                return false;
            }
            
        }
    }
}