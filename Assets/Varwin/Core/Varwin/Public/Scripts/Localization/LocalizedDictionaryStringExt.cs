using Varwin.Core;

namespace Varwin.Public
{
    public static class LocalizedDictionaryStringExt
    {
        public static I18n ToI18n(this LocalizedDictionary<string> localizedDictionary)
        {
            var i18N = new I18n();
            foreach (var language in localizedDictionary.GetLanguages())
            {
                i18N.SetLocale(language.GetCode(), localizedDictionary.GetValue(language));
            }

            return i18N;
        }
    }
}