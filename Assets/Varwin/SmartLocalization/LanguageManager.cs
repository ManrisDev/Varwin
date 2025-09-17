using System;
using UnityEngine;

namespace SmartLocalization
{

    public class LanguageManager : MonoBehaviour
    {
        public delegate void ChangeLanguageEventHandler(LanguageManager thisLanguageManager);
        public ChangeLanguageEventHandler OnChangeLanguage;
        
        public static LanguageManager Instance { get; set; }

        public string GetTextValue(string key) => key;

        public SmartCultureInfo CurrentlyLoadedCulture { get; set; }
        public static string DefaultLanguage { get; set; }

        public static void SetDontDestroyOnLoad()
        {
        }

        public void ChangeLanguage(string launchArgumentsLang)
        {
        }

        private void Awake()
        {
            if (Instance)
            {
                DestroyImmediate(this);
            }
            else
            {
                CurrentlyLoadedCulture =  new SmartCultureInfo();
                Instance = this;
            }
        }
    }
}