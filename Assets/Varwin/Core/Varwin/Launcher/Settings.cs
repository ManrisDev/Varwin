using System;
using System.Globalization;
using System.IO;
using Varwin.Log;
using Newtonsoft.Json;
using SmartLocalization;
using UnityEngine;
using Varwin.Data;
using Varwin.Data.ServerData;
#if VARWINCLIENT
using Varwin.UI;
#endif

namespace Varwin
{
    public class Settings
    {
        public string ApiHost { get; set; }

        // For test purposes in Unity Editor. Change in settings.txt file. Values: {debug - run with ctrl + ~, debug1 - run with ctrl + 1}
        public string Language { get; set; }
        public string StoragePath { get; set; }
        public string DebugFolder { get; set; }
        public string WebHost { get; set; }
        public string RemoteAddress { get; set; }
        public string RemoteAddressPort { get; set; }
        public string RemoteWebHost { get; set; }
        public bool Multiplayer { get; set; }
        public bool Spectator { get; set; }
        public bool Education { get; set; }
        public bool HighlightEnabled { get; set; }
        public bool TouchHapticsEnabled { get; set; }
        public bool GrabHapticsEnabled { get; set; }
        public bool UseHapticsEnabled { get; set; }
        [Obsolete] public bool OnboardingMode { get; set; }

        private static Settings _instance;

        public static Settings Instance
        {
            get => _instance ??= new Settings
            {
                HighlightEnabled = true,
                TouchHapticsEnabled = false,
                GrabHapticsEnabled = false,
                UseHapticsEnabled = false,
                Multiplayer = false,
                Education = true,
                OnboardingMode = false,
                Language = LanguageManager.DefaultLanguage
            };
            set => _instance = value;
        }

        public Settings()
        {
            if (LanguageManager.Instance)
            {
                LanguageManager.Instance.OnChangeLanguage += UpdateLanguage;
            }
        }

        private void UpdateLanguage(LanguageManager languageManager)
        {
            Language = languageManager.CurrentlyLoadedCulture.languageCode;
        }

        [Obsolete]
        public static void ReadTestSettings()
        {
        }

        // build
        public static void CreateStorageSettings(string folder)
        {
            Debug.Log($"Create storage settings. Path = {folder}");
            Instance = new Settings
            {
                StoragePath = folder,
                HighlightEnabled = true,
                Language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
#if WAVEVR
                Education = true,
                Multiplayer = true
#endif
            };
        }

        [Obsolete]
        public static void CreateDebugSettings(string path)
        {
        }

#if VARWINCLIENT
        public static void SetupLanguageFromLaunchArguments(LaunchArguments launchArguments)
        {
            try
            {
                Instance.Language = launchArguments.lang ?? _instance.Language ?? LanguageManager.DefaultLanguage;

                LanguageManager.Instance.ChangeLanguage(launchArguments.lang);
            }
            catch (Exception e)
            {
                LauncherErrorManager.Instance.ShowFatal(ErrorHelper.GetErrorDescByCode(ErrorCode.ReadLaunchArgsError), e.ToString());
            }
        }
#endif

        public static void ReadServerConfig(ServerConfig serverConfig)
        {
            var uri = new Uri(Instance.ApiHost);

            Instance.WebHost = $@"{Instance.ApiHost}/widgets";
            Instance.RemoteAddress = serverConfig.remoteAddr;
            Instance.RemoteAddressPort = serverConfig.remoteAddrPort;
#if VARWINCLIENT
            LicenseFeatureManager.ActivateLicenseFeatures(serverConfig.appLicenseInfo.Edition ?? Edition.None);
#endif

            Instance.RemoteWebHost = $@"{uri.Scheme}://{Instance.RemoteAddress}:{Instance.RemoteAddressPort}";

            Instance.Language = Instance.Language ?? LanguageManager.DefaultLanguage;
        }

        public static void SetLanguage(string language)
        {
            if (language == "auto")
            {
                language = LanguageManager.DefaultLanguage;
            }
            
            LanguageManager.Instance.ChangeLanguage(language);
            Instance.Language = language;
        }

        public static void SetApiUrl(string url)
        {
            Instance.ApiHost = url;
        }
    }
}