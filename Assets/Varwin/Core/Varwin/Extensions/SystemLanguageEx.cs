using UnityEngine;

namespace Varwin.Core
{
    public static class SystemLanguageEx
    {
        public static string GetCode(this Language language)
        {
            switch (language)
            {
                case Language.Chinese:
                    return "cn";
                case Language.ChineseSimplified:
                    return "cn_s";
                case Language.ChineseTraditional:
                    return "cn_t";
                case Language.Japanese:
                    return "jp";
                case Language.Portuguese:
                    return "pg";
                case Language.Slovak:
                    return "sk";
                case Language.Slovenian:
                    return "sv";
                case Language.Indonesian:
                    return "ind";
                case Language.Kazakh:
                    return "kk";
                default:
                    return language.ToString().Substring(0, 2).ToLowerInvariant();
            }
        }
        
        public static string GetCode(this SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.Chinese:
                    return "cn";
                case SystemLanguage.ChineseSimplified:
                    return "cn_s";
                case SystemLanguage.ChineseTraditional:
                    return "cn_t";
                case SystemLanguage.Japanese:
                    return "jp";
                case SystemLanguage.Portuguese:
                    return "pg";
                case SystemLanguage.Slovak:
                    return "sk";
                case SystemLanguage.Slovenian:
                    return "sv";
                case SystemLanguage.Indonesian:
                    return "ind";
                default:
                    return language.ToString().Substring(0, 2).ToLowerInvariant();
            }
        }
    }
}