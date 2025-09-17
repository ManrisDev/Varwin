using System;
using System.Collections.Generic;
using System.Linq;
using SmartLocalization;
using UnityEngine;
#if VARWINCLIENT
using Varwin.UI;
using Varwin.UI.VRErrorManager;
#endif
namespace Varwin.Log
{
    public static class ErrorHelper
    {
        private static Dictionary<int, string> _errCodesToLocStrings = new Dictionary<int, string>();

        private enum DocumentedErrorTypes
        {
            LocalServerConnectionError,
            RemoteServerConnectionError,
            LocalServerParseError,
            RemoteServerParseError
        }

        private static Dictionary<DocumentedErrorTypes, string> _errorCodeToDocLink = new()
        {
            [DocumentedErrorTypes.LocalServerConnectionError] = "https://docs.varwin.com/latest/ru/faq-ustanovka-i-zapusk-programmy-2260862579.html",
            [DocumentedErrorTypes.RemoteServerConnectionError] = "https://docs.varwin.com/latest/ru/faq-ustanovka-i-zapusk-programmy-2260862579.html",
            [DocumentedErrorTypes.LocalServerParseError] = "https://docs.varwin.com/latest/ru/faq-ustanovka-i-zapusk-programmy-2260862579.html",
            [DocumentedErrorTypes.RemoteServerParseError] = "https://docs.varwin.com/latest/ru/faq-ustanovka-i-zapusk-programmy-2260862579.html",
        };

        static ErrorHelper()
        {
            _errCodesToLocStrings[ErrorCode.CompileCodeError] = "COMPILE_CODE_ERROR";
            _errCodesToLocStrings[ErrorCode.RuntimeCodeError] = "RUNTIME_CODE_ERROR";
            _errCodesToLocStrings[ErrorCode.ReadLaunchArgsError] = "ERROR_READ_STARTUP_ARGS";
            _errCodesToLocStrings[ErrorCode.ServerNoConnectionError] = "ERROR_SERVER_DISCONNECT";
            _errCodesToLocStrings[ErrorCode.ApiAndClientVersionMismatchError] = "ERROR_API_CLIENT_VERSION_MISMATCH";
            _errCodesToLocStrings[ErrorCode.LogicExecuteError] = "EXECUTE_LOGIC_ERROR";
            _errCodesToLocStrings[ErrorCode.LogicInitError] = "INIT_LOGIC_ERROR";
            _errCodesToLocStrings[ErrorCode.LicenseKeyError] = "LICENSE_KEY_ERROR";
            _errCodesToLocStrings[ErrorCode.LoadObjectError] = "LOAD_OBJECT_ERROR";
            _errCodesToLocStrings[ErrorCode.LoadSceneError] = "LOAD_SCENE_ERROR";
            _errCodesToLocStrings[ErrorCode.RabbitNoArgsError] = "READ_LAUNCH_ARGS_RABBIT_ERROR";
            _errCodesToLocStrings[ErrorCode.SaveSceneError] = "SAVE_ERROR";
            _errCodesToLocStrings[ErrorCode.SpawnPointNotFoundError] = "SPAWN_POINT_ERROR";
            _errCodesToLocStrings[ErrorCode.UnknownError] = "UNKNOWN_ERROR";
            _errCodesToLocStrings[ErrorCode.LoadWorldConfigError] = "WORLD_CONFIG_ERROR";
            _errCodesToLocStrings[ErrorCode.ProjectConfigNullError] = "WORLD_CONFIG_NULL";
            _errCodesToLocStrings[ErrorCode.ExceptionInObject] = "EXCEPTION_IN_OBJECT";
            _errCodesToLocStrings[ErrorCode.CannotPreview] = "CANNOT_NOT_PREVIEW";
            _errCodesToLocStrings[ErrorCode.NotForCommercialUse] = "NOT_FOR_COMMERCIAL_USE";
            _errCodesToLocStrings[ErrorCode.EnvironmentNotFoundError] = "ENVIRONMENT_ERROR";
            _errCodesToLocStrings[ErrorCode.MobileVRIsNotAvailable] = "MAINMENU_MOBILE_VR_IS_NOT_AVAILABLE";
        }

        private static readonly List<string> IgnoreStackTrace = new()
        {
            "Varwin.Core",
            "Varwin.WWW",
            "Varwin.ECS",
            "Varwin.ProjectDataListener",
            "Entitas.",
            "NLog.",
            "Assets/Core/",
            "SteamVR_Action",
            "UsedWindowsBundlesOnLinuxHandler"
        };

        private static readonly List<string> IgnoreMessages = new()
        {
            "Unsupported encoding",
            "Can't play movie []",
            "Screen position out of view frustum",
            "are not multiples of 4. Compress will not work.",
            "[Varwin OpenXR] Create instance failed",
            "Triggers on concave MeshColliders are not supported",
            "Releasing render texture that is set as Camera.targetTexture!"
        };

        private static readonly List<string> ErrorPopupIgnoreMessages = new()
        {
            "[Varwin OpenXR] Create instance failed. Error: ErrorRuntimeUnavailable"
        };

        public static string GetErrorKeyByCode(int errorCode) => _errCodesToLocStrings[errorCode];
        
        public static string GetErrorDescByCode(int errorCode) =>
            LanguageManager.Instance.GetTextValue(_errCodesToLocStrings[errorCode]);

        public static void DisplayErrorByCode(int errorCode)
        {
#if VARWINCLIENT
            string errorMessage =
                $"Error {errorCode}.\n {LanguageManager.Instance.GetTextValue(_errCodesToLocStrings[errorCode])}";

            if (VRErrorManager.Instance)
            {
                VRErrorManager.Instance.ShowFatal(errorMessage);
            }
            
            LauncherErrorManager launcherErrorManager = LauncherErrorManager.Instance;

            if (launcherErrorManager != null)
            {
                launcherErrorManager.ShowFatal(errorMessage, string.Empty);
            }
#endif
        }

        public static void ErrorHandler(string condition, string stackTrace, LogType logType)
        {
#if VARWINCLIENT
            if (logType != LogType.Exception)
            {
                return;
            }

            bool errorInObject = stackTrace.Contains("Varwin.Types");
            var launcherErrorManager = LauncherErrorManager.Instance;
            var vrErrorManager = VRErrorManager.Instance;

            if (!errorInObject)
            {
                string userMessage = GetErrorDescByCode(ErrorCode.UnknownError);
                string errorMessage =
                    $"<b>{condition.Trim()}</b>{Environment.NewLine}{Environment.NewLine}{stackTrace}";

                userMessage += "\n" + condition;

                if (launcherErrorManager != null)
                {
                    launcherErrorManager.ShowFatal(userMessage, errorMessage);
                }

                if (vrErrorManager != null)
                {
                    vrErrorManager.ShowFatal(userMessage, errorMessage);
                }

                Debug.LogError("Unknown error! " + condition + " stackTrace = " + stackTrace);
            }
            else
            {
                string userMessage = LanguageManager.Instance.GetTextValue("EXCEPTION_IN_OBJECT");
                userMessage += " " + stackTrace.Split('\n')[0];

                if (vrErrorManager != null)
                {
                    CoreErrorManager.Error(new Exception(userMessage));
                    vrErrorManager.Show(userMessage);
                }

                Debug.LogError("Object error! " + condition + " stackTrace = " + stackTrace);
            }
#endif
        }

        public static bool NeedIgnoreStackTrace(string stackTrace, LogType logType)
        {
            return logType is not (LogType.Error or LogType.Exception) && ListContainsString(IgnoreStackTrace, stackTrace);
        }

        public static bool NeedIgnoreMessage(string message)
        {
            return ListContainsString(IgnoreMessages, message);
        }

        public static bool IsPopupIgnoreMessage(string message)
        {
            return ListContainsString(ErrorPopupIgnoreMessages, message);
        }

        /// <summary>
        /// Попытаться получить строку ошибки подключения к серверу.
        /// </summary>
        /// <param name="uriString">Uri запроса.</param>
        /// <param name="localizedError">Локализованная ошибка.</param>
        /// <param name="documentationLink">Ссылка на документацию.</param>
        /// <returns>Удалось ли определить ошибку сервера.</returns>
        public static string TryGetServerConnectionError(string uriString, out string documentationLink, out bool isConnectionExists)
        {
            const int timeout = 3000;
            documentationLink = null;

            isConnectionExists = UriHelper.IsConnectionExists(uriString, timeout);
            if (UriHelper.IsLocal(uriString))
            {
                documentationLink = _errorCodeToDocLink[DocumentedErrorTypes.LocalServerConnectionError];
                return LanguageManager.Instance.GetTextValue("ERROR_LOCAL_SERVER_CONNECTION");
            }

            documentationLink = documentationLink = _errorCodeToDocLink[DocumentedErrorTypes.RemoteServerConnectionError];
            return LanguageManager.Instance.GetTextValue("ERROR_REMOTE_SERVER_CONNECTION");
        }

        /// <summary>
        /// Ошибка при получении данных с сервера. (Неверный формат данных).
        /// </summary>
        /// <param name="uriString">Uri запроса.</param>
        /// <param name="documentationLink">Ссылка на документацию.</param>
        /// <returns>Локализованная ошибка.</returns>
        public static string GetParseServerConfigError(string uriString, out string documentationLink)
        {
            if (UriHelper.IsLocal(uriString))
            {
                documentationLink = _errorCodeToDocLink[DocumentedErrorTypes.LocalServerParseError];
                return LanguageManager.Instance.GetTextValue("ERROR_LOCAL_SERVER_PARSE_CONFIG");
            }

            documentationLink = _errorCodeToDocLink[DocumentedErrorTypes.RemoteServerParseError];
            return LanguageManager.Instance.GetTextValue("ERROR_REMOTE_SERVER_PARSE_CONFIG");
        }
        
        private static bool ListContainsString(IEnumerable<string> ignoreDictionary, string message)
        {
            return ignoreDictionary.Any(filter => message.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0);
        }

        private static string GetDocumentationLocale()
        {
            var languageCode = LanguageManager.Instance.CurrentlyLoadedCulture.languageCode;
            if (languageCode == "ru")
            {
                return languageCode;
            }

            return "en";
        }
    }
}
