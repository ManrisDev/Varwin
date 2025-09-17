using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Varwin;

public static class I18nEx
{
    public const string GetLocaleErrorMessage = "ERROR: NO VALID STRING";

    public static string LocalizedString(this I18n self)
    {
        string lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        
#if VARWINCLIENT
        lang = Settings.Instance.Language;
#endif
        Type localeType = typeof(I18n);
        string value = localeType.GetProperty(lang)?.GetValue(self)?.ToString();

        if (!string.IsNullOrEmpty(value))
        {
            return value;
        }

        if (!string.IsNullOrEmpty(self.en))
        {
            value = self.en;
        }
        else
        {
            var properties = localeType.GetProperties();

            value = properties.FirstOrDefault(x => x.CanRead && string.IsNullOrEmpty(x.GetValue(self)?.ToString()))
                ?.ToString();
        }

        return value ?? string.Empty;
    }

    public static void SetLocale(this I18n self, string lang, string value)
    {
        typeof(I18n).GetProperty(lang)?.SetValue(self, value);
    }

    public static void SetLocale(this ILocalizable self, string lang, string value)
    {
        if (self.i18n == null)
        {
            self.i18n = new I18n();
        }

        self.i18n.SetLocale(lang, value);
    }

    public static string GetCurrentLocale(this I18n self)
    {
        var result = string.Empty;
        string lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        
#if VARWINCLIENT
        lang = Settings.Instance.Language;
#endif

        var property = typeof(I18n).GetProperty(lang);

        if (property == null)
        {
            return result;
        }

        var localizedString = property.GetValue(self)?.ToString();
        if (string.IsNullOrEmpty(localizedString))
        {
            return !string.IsNullOrEmpty(self.en) ? self.en : GetLocaleErrorMessage;
        }

        return localizedString;
    }
}
