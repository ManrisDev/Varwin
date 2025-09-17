using System;
using System.Net;

namespace Varwin
{
    /// <summary>
    /// Хелпер для работы с ссылками.
    /// </summary>
    public static class UriHelper
    {
        /// <summary>
        /// Проверка, является ли ссылка локальной или удаленной.
        /// </summary>
        /// <param name="uriString">Ссылка.</param>
        /// <returns>Является ли ссылка локальной.</returns>
        public static bool IsLocal(string uriString)
        {
            if (!IsValidUri(uriString))
            {
                return false;
            }

            var uri = new Uri(uriString);
            var host = uri.Host;

            if (host == "localhost" || host == "127.0.0.1")
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Проверка валидности ссылки.
        /// </summary>
        /// <param name="uriString">Ссылка.</param>
        /// <returns>Является ли ссылка валидной.</returns>
        public static bool IsValidUri(string uriString) => Uri.IsWellFormedUriString(uriString, UriKind.RelativeOrAbsolute);

        /// <summary>
        /// Проверка на сущствование подключения.
        /// </summary>
        /// <param name="uriString">Ссылка.</param>
        /// <param name="timeoutMs">Таймаут проверки.</param>
        /// <returns>Существует ли подключение или нет.</returns>
        public static bool IsConnectionExists(string uriString, int timeoutMs = 5000)
        {
            if (!IsValidUri(uriString))
            {
                return false;
            }

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uriString);
                request.KeepAlive = false;
                request.Timeout = timeoutMs;

                using var response = (HttpWebResponse)request.GetResponse();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}