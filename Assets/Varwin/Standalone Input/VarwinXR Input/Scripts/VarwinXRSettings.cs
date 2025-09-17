using UnityEngine;

namespace Varwin.XR
{
    /// <summary>
    /// Класс настроек VarwinXR.
    /// </summary>
    public static class VarwinXRSettings
    {
        /// <summary>
        /// Имя эмулируемого шлема.
        /// </summary>
        private const string EmulatedHeadsetNameKey = "[EmulatedHeadsetName]";
        
        /// <summary>
        /// Использование нативного направления указки.
        /// </summary>
        private const string UseVarwinPointerDirectionKey = "[UseVarwinPointerDirection]";

        /// <summary>
        /// Делегат события смены имени шлема.
        /// </summary>
        /// <param name="name">Новое имя шлема.</param>
        public delegate void HeadsetNameEventHandler(string name);
        
        /// <summary>
        /// Делегат события смены режима указки.
        /// </summary>
        /// <param name="useNativeDirection">Новое состояние режима указки.</param>
        public delegate void UseVarwinPointerDirectionHandler(bool useNativeDirection);

        /// <summary>
        /// Событие, вызываемое при смене эмулированного шлема.
        /// </summary>
        public static event HeadsetNameEventHandler HeadsetNameChanged;
        
        /// <summary>
        /// Событие, вызываемое при смене режима указки.
        /// </summary>
        public static event UseVarwinPointerDirectionHandler UseVarwinPointerDirectionChanged;

        /// <summary>
        /// Проинициализировано ли поле эмулированного шлема.
        /// </summary>
        private static bool _isEmulatedHeadsetNameFieldInited = false;
        
        /// <summary>
        /// Проинициализировано ли поле направления указки.
        /// </summary>
        private static bool _isUseVarwinPointerDirectionFieldInited = false;
        
        /// <summary>
        /// Имя эмулируемого шлема.
        /// </summary>
        private static string _emulatedHeadsetName;
        
        /// <summary>
        /// Использовать нативное расположение указки.
        /// </summary>
        private static bool _useVarwinPointerDirection;
        
        /// <summary>
        /// Имя эмулируемого шлема.
        /// </summary>
        public static string EmulatedHeadsetName
        {
            get
            {
                if (_isEmulatedHeadsetNameFieldInited)
                {
                    return _emulatedHeadsetName;
                }
                
                _emulatedHeadsetName = PlayerPrefs.GetString(EmulatedHeadsetNameKey);
                _isEmulatedHeadsetNameFieldInited = true;

                return _emulatedHeadsetName;
            }
            set
            {
                PlayerPrefs.SetString(EmulatedHeadsetNameKey, value);
                _emulatedHeadsetName = value;
                HeadsetNameChanged?.Invoke(value);
            
                _isEmulatedHeadsetNameFieldInited = true;
            }
        }

        /// <summary>
        /// Использовать нативное расположение указки.
        /// </summary>
        public static bool UseVarwinPointerDirection
        {
            get
            {
                if (_isUseVarwinPointerDirectionFieldInited)
                {
                    return _useVarwinPointerDirection;
                }
                
                _useVarwinPointerDirection = PlayerPrefs.GetInt(UseVarwinPointerDirectionKey) > 0;
                _isUseVarwinPointerDirectionFieldInited = true;

                return _useVarwinPointerDirection;
            }
            set
            {
                PlayerPrefs.SetInt(UseVarwinPointerDirectionKey, value ? 1 : 0);
                _useVarwinPointerDirection = value;
                _isUseVarwinPointerDirectionFieldInited = true;
                
                UseVarwinPointerDirectionChanged?.Invoke(value);
            }
        }
    }
}