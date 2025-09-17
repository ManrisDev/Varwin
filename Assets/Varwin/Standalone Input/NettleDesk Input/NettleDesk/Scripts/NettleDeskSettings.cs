using System;
using UnityEngine;

namespace Varwin.NettleDeskPlayer
{
    public static class NettleDeskSettings
    {
        private const string UseStylusSettingsKey = "NettleDeskUseStylus";
        public static event Action SettingsChanged;
        
        private static bool _initialized;
        private static bool _useStylus;
        
        public static bool StylusSupport
        {
            get
            {
                if (_initialized)
                {
                    return _useStylus;
                }

                _useStylus = PlayerPrefs.GetInt(UseStylusSettingsKey) == 1;
                _initialized = true;
                return _useStylus;
            }
            set
            {
                PlayerPrefs.SetInt(UseStylusSettingsKey, value ? 1 : 0);
                _useStylus = value;
                SettingsChanged?.Invoke();
            }
        }
    }
}