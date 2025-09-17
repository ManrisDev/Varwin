using UnityEngine;

namespace Varwin
{
    public static class FileSystemUtilsBlocklyWrapper
    {
        private static float _openRate = 10;
        private static long _openCounter = 0;
        private static float _lastOpenTime = 0;
        private static bool _locked = false;

        private static string[] _forbiddenExtensions = {"exe", "bat", "cmd", "js", "jse", "vbs", "vbe", "vb", "com", "msi", "msu", "ps1", "scr"};
        private static string _className = nameof(FileSystemUtilsBlocklyWrapper);

        public static void OpenURL(string url)
        {
            if (_locked)
            {
                return;
            }

            CheckOpenRate();

            FileSystemUtils.OpenURL(url);
        }

        public static void OpenFile(string path)
        {
            if (_locked)
            {
                return;
            }

            CheckOpenRate();

            var lowercase = path.Trim().ToLower();

            foreach (var forbiddenExtension in _forbiddenExtensions)
            {
                if (!lowercase.EndsWith(forbiddenExtension))
                {
                    continue;
                }

                Debug.LogError($"{_className}: Opening {forbiddenExtension} is disabled for security reasons");
                return;
            }

            FileSystemUtils.OpenFile(path);
        }

        /// <summary>
        /// Метод проверки частоты открытия файлов или ссылок. Необходим для случая, если юзер случайно закинет блок в Update
        /// </summary>
        private static void CheckOpenRate()
        {
            var currentRate = 1 / Mathf.Max(Time.time - _lastOpenTime, 0.001f);
            _openRate = Mathf.Lerp(currentRate, _openRate, 0.5f);
            _lastOpenTime = Time.time;

            if (_openCounter > 10 && _openRate > 1)
            {
                _locked = true;
                Debug.LogError($"{_className}: Opening files happens too often. Action will be disabled for security reasons.");
            }

            _openCounter++;
        }
    }
}