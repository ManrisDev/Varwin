using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Varwin.Core;
using Varwin.Public;

namespace Varwin
{
    public static class LocalizationUtils
    {
        private static readonly Dictionary<Type, string> TypeToPrefixKeyMap = new()
        {
            [typeof(ActionAttribute)] = "BLOCK_ACTION_PREFIX",
            [typeof(FunctionAttribute)] = "BLOCK_FUNCTION_PREFIX",
            [typeof(LogicEventAttribute)] = "BLOCK_EVENT_PREFIX",
            [typeof(CheckerAttribute)] = "BLOCK_CHECKER_PREFIX",
            [typeof(VariableAttribute)] = "BLOCK_SETTER_PREFIX",
            [typeof(EventAttribute)] = "BLOCK_EVENT_PREFIX",
            [typeof(GetterAttribute)] = "BLOCK_GETTER_PREFIX",
            [typeof(SetterAttribute)] = "BLOCK_SETTER_PREFIX"
        };

        private static readonly Dictionary<Type, string> TypeToLocalizedTypeKey = new()
        {
            [typeof(int)] = "TYPE_INTEGER",
            [typeof(long)] = "TYPE_INTEGER",
            [typeof(short)] = "TYPE_INTEGER",
            [typeof(byte)] = "TYPE_INTEGER",
            [typeof(float)] = "TYPE_DOUBLE",
            [typeof(double)] = "TYPE_DOUBLE",
            [typeof(decimal)] = "TYPE_DOUBLE",
            [typeof(bool)] = "TYPE_BOOLEAN",
            [typeof(Vector3)] = "TYPE_VECTOR",
            [typeof(Color)] = "TYPE_COLOR",
            [typeof(string)] = "TYPE_STRING",
            [typeof(String)] = "TYPE_STRING",
            [typeof(Wrapper)] = "TYPE_WRAPPER",
        };

        public static List<LocalizationString> GetLocalizationStrings(Component component)
        {
            return GetLocalizationStrings(component.GetType());
        }
        
        public static List<LocalizationString> GetLocalizationStrings(Type type)
        {
            var localizationStrings = new List<LocalizationString>();
            
            var locales = type.GetCustomAttributes<LocaleAttribute>(true);

            var localeAttributes = locales as LocaleAttribute[] ?? locales.ToArray();

            if (localeAttributes.Length > 0)
            {
                foreach (var locale in localeAttributes)
                {
                    if (locale.I18n != null)
                    {
                        return GetLocalizationStringsFromI18n(locale.I18n);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(locale.Strings[0]))
                        {
                            localizationStrings.Add(new LocalizationString((Language)locale.Language, locale.Strings[0].Trim()));
                        }
                    }
                }
            }
            else
            {
                var objectName = type.GetCustomAttribute<ObjectNameAttribute>(true);

                if (objectName != null)
                {
                    return GetLocalizationStringsFromI18n(objectName.LocalizedNames);
                }
                else
                {
                    var varwinComponent = type.GetCustomAttribute<VarwinComponentAttribute>(true);
                    if (varwinComponent != null)
                    {
                        return GetLocalizationStringsFromI18n(varwinComponent.LocalizedNames);
                    }
                }
            }

            return localizationStrings;
        }

        public static I18n GetI18n(Component component)
        {
            return GetI18n(component.GetType());
        }

        public static I18n GetI18n(Type type)
        {
            var i18n = new I18n();
            
            var locales = type.GetCustomAttributes<LocaleAttribute>(true);
            var localeAttributes = locales as LocaleAttribute[] ?? locales.ToArray();
            if (localeAttributes.Length > 0)
            {
                foreach (var locale in localeAttributes)
                {
                    if (locale.I18n != null)
                    {
                        return locale.I18n;
                    }
                    else
                    {
                        i18n.SetLocale(locale.Code, locale.Strings[0]);
                    }
                }
            }
            else
            {
                var objectName = type.GetCustomAttribute<ObjectNameAttribute>(true);
                if (objectName != null)
                {
                    return objectName.LocalizedNames;
                }
                else
                {
                    var varwinComponent = type.GetCustomAttribute<VarwinComponentAttribute>(true);
                    if (varwinComponent != null)
                    {
                        return varwinComponent.LocalizedNames;
                    }
                }
            }

            return i18n;
        }

        // Used in sdk
        public static I18n ReplaceEmptyWithWhiteSpace(I18n locale)
        {
            return new I18n
            {
                en = EmptyToWhiteSpace(locale.en),
                af = EmptyToWhiteSpace(locale.af),
                ar = EmptyToWhiteSpace(locale.ar),
                ba = EmptyToWhiteSpace(locale.ba),
                be = EmptyToWhiteSpace(locale.be),
                bu = EmptyToWhiteSpace(locale.bu),
                ca = EmptyToWhiteSpace(locale.ca),
                cn = EmptyToWhiteSpace(locale.cn),
                cz = EmptyToWhiteSpace(locale.cz),
                da = EmptyToWhiteSpace(locale.da),
                du = EmptyToWhiteSpace(locale.du),
                es = EmptyToWhiteSpace(locale.es),
                fa = EmptyToWhiteSpace(locale.fa),
                fi = EmptyToWhiteSpace(locale.fi),
                fr = EmptyToWhiteSpace(locale.fr),
                ge = EmptyToWhiteSpace(locale.ge),
                gr = EmptyToWhiteSpace(locale.gr),
                he = EmptyToWhiteSpace(locale.he),
                hu = EmptyToWhiteSpace(locale.hu),
                ic = EmptyToWhiteSpace(locale.ic),
                ind = EmptyToWhiteSpace(locale.ind),
                it = EmptyToWhiteSpace(locale.it),
                jp = EmptyToWhiteSpace(locale.jp),
                ko = EmptyToWhiteSpace(locale.ko),
                la = EmptyToWhiteSpace(locale.la),
                li = EmptyToWhiteSpace(locale.li),
                no = EmptyToWhiteSpace(locale.no),
                po = EmptyToWhiteSpace(locale.po),
                pg = EmptyToWhiteSpace(locale.pg),
                ro = EmptyToWhiteSpace(locale.ro),
                ru = EmptyToWhiteSpace(locale.ru),
                se = EmptyToWhiteSpace(locale.se),
                sk = EmptyToWhiteSpace(locale.sk),
                sv = EmptyToWhiteSpace(locale.sv),
                sp = EmptyToWhiteSpace(locale.sp),
                sw = EmptyToWhiteSpace(locale.sw),
                th = EmptyToWhiteSpace(locale.th),
                tu = EmptyToWhiteSpace(locale.tu),
                uk = EmptyToWhiteSpace(locale.uk),
                vi = EmptyToWhiteSpace(locale.vi),
                cn_s = EmptyToWhiteSpace(locale.cn_s),
                cn_t = EmptyToWhiteSpace(locale.cn_t),
                kk = EmptyToWhiteSpace(locale.kk),
            };
        }

        private static string EmptyToWhiteSpace(string locale) => locale == string.Empty ? " " : locale;

        public static List<LocalizationString> GetLocalizationStringsFromI18n(I18n i18n)
        {
            var localizationStrings = new List<LocalizationString>();
            
            if (!string.IsNullOrWhiteSpace(i18n.en))
            {
                localizationStrings.Add(new LocalizationString(Language.English, i18n.en.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.ru))
            {
                localizationStrings.Add(new LocalizationString(Language.Russian, i18n.ru.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.af))
            {
                localizationStrings.Add(new LocalizationString(Language.Afrikaans, i18n.af.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.ar))
            {
                localizationStrings.Add(new LocalizationString(Language.Arabic, i18n.ar.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.ba))
            {
                localizationStrings.Add(new LocalizationString(Language.Basque, i18n.ba.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.be))
            {
                localizationStrings.Add(new LocalizationString(Language.Belarusian, i18n.be.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.bu))
            {
                localizationStrings.Add(new LocalizationString(Language.Bulgarian, i18n.bu.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.ca))
            {
                localizationStrings.Add(new LocalizationString(Language.Catalan, i18n.ca.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.cn))
            {
                localizationStrings.Add(new LocalizationString(Language.Chinese, i18n.cn.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.cn_s))
            {
                localizationStrings.Add(new LocalizationString(Language.ChineseSimplified, i18n.cn_s.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.cn_t))
            {
                localizationStrings.Add(new LocalizationString(Language.ChineseTraditional, i18n.cn_t.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.cz))
            {
                localizationStrings.Add(new LocalizationString(Language.Czech, i18n.cz.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.da))
            {
                localizationStrings.Add(new LocalizationString(Language.Danish, i18n.da.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.du))
            {
                localizationStrings.Add(new LocalizationString(Language.Dutch, i18n.du.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.es))
            {
                localizationStrings.Add(new LocalizationString(Language.Estonian, i18n.es.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.fa))
            {
                localizationStrings.Add(new LocalizationString(Language.Faroese, i18n.fa.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.fi))
            {
                localizationStrings.Add(new LocalizationString(Language.Finnish, i18n.fi.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.fr))
            {
                localizationStrings.Add(new LocalizationString(Language.French, i18n.fr.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.ge))
            {
                localizationStrings.Add(new LocalizationString(Language.German, i18n.ge.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.gr))
            {
                localizationStrings.Add(new LocalizationString(Language.Greek, i18n.gr.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.he))
            {
                localizationStrings.Add(new LocalizationString(Language.Hebrew, i18n.he.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.hu))
            {
                localizationStrings.Add(new LocalizationString(Language.Hungarian, i18n.hu.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.ic))
            {
                localizationStrings.Add(new LocalizationString(Language.Icelandic, i18n.ic.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.it))
            {
                localizationStrings.Add(new LocalizationString(Language.Italian, i18n.it.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.jp))
            {
                localizationStrings.Add(new LocalizationString(Language.Japanese, i18n.jp.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.ko))
            {
                localizationStrings.Add(new LocalizationString(Language.Korean, i18n.ko.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.la))
            {
                localizationStrings.Add(new LocalizationString(Language.Latvian, i18n.la.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.li))
            {
                localizationStrings.Add(new LocalizationString(Language.Lithuanian, i18n.li.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.no))
            {
                localizationStrings.Add(new LocalizationString(Language.Norwegian, i18n.no.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.pg))
            {
                localizationStrings.Add(new LocalizationString(Language.Portuguese, i18n.pg.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.po))
            {
                localizationStrings.Add(new LocalizationString(Language.Polish, i18n.po.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.ro))
            {
                localizationStrings.Add(new LocalizationString(Language.Romanian, i18n.ro.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.se))
            {
                localizationStrings.Add(new LocalizationString(Language.SerboCroatian, i18n.se.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.sk))
            {
                localizationStrings.Add(new LocalizationString(Language.Slovak, i18n.sk.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.sp))
            {
                localizationStrings.Add(new LocalizationString(Language.Spanish, i18n.sp.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.sv))
            {
                localizationStrings.Add(new LocalizationString(Language.Slovenian, i18n.sv.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.sw))
            {
                localizationStrings.Add(new LocalizationString(Language.Swedish, i18n.sw.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.th))
            {
                localizationStrings.Add(new LocalizationString(Language.Thai, i18n.th.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.tu))
            {
                localizationStrings.Add(new LocalizationString(Language.Turkish, i18n.tu.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.uk))
            {
                localizationStrings.Add(new LocalizationString(Language.Ukrainian, i18n.uk.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.vi))
            {
                localizationStrings.Add(new LocalizationString(Language.Vietnamese, i18n.vi.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.ind))
            {
                localizationStrings.Add(new LocalizationString(Language.Indonesian, i18n.ind.Trim()));
            }
            
            if (!string.IsNullOrWhiteSpace(i18n.kk))
            {
                localizationStrings.Add(new LocalizationString(Language.Kazakh, i18n.kk.Trim()));
            }
            
            return localizationStrings;
        }

        public static I18n GetI18nFromLocalizationStrings(List<LocalizationString> localizationStrings)
        {
            var i18n = new I18n();
            foreach (var displayName in localizationStrings)
            {
                i18n.SetLocale(displayName.key.GetCode(), displayName.value);
            }

            return i18n;
        }

        /// <summary>
        /// Получить префикс блока (установить, получить, и т.д.).
        /// </summary>
        /// <param name="blockType"></param>
        /// <returns>Локализованный префикс.</returns>
        public static string GetLocalizedBlockPrefix(Type blockType)
        {
            if (blockType == null || !TypeToPrefixKeyMap.TryGetValue(blockType, out string localizedPrefixKey))
            {
                return string.Empty;
            }

            return SmartLocalization.LanguageManager.Instance.GetTextValue(localizedPrefixKey);
        }

        /// <summary>
        /// Локализация типа.
        /// </summary>
        /// <param name="type">Тип.</param>
        /// <returns>Локализованное имя типа. Если локализация не удалась, то возвращается Type.Name.</returns>
        public static string GetLocalizedType(Type type)
        {
            if (type == null)
            {
                return SmartLocalization.LanguageManager.Instance.GetTextValue("TYPE_NULL");
            }

            if (TypeToLocalizedTypeKey.TryGetValue(type, out var typeLocalizationKey))
            {
                return SmartLocalization.LanguageManager.Instance.GetTextValue(typeLocalizationKey);
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                if (type.IsArray)
                {
                    Type elementType = type.GetElementType();
                    if (elementType == null)
                    {
                        return string.Empty;
                    }

                    var localizedArrayType = SmartLocalization.LanguageManager.Instance.GetTextValue("TYPE_ARRAY");
                    return string.Format(localizedArrayType, GetLocalizedType(elementType));
                }

                if (typeof(IList).IsAssignableFrom(type))
                {
                    var listElementType = type.GetGenericArguments().FirstOrDefault();
                    var localizedListType = SmartLocalization.LanguageManager.Instance.GetTextValue("TYPE_LIST");

                    if (listElementType == null)
                    {
                        return SmartLocalization.LanguageManager.Instance.GetTextValue("TYPE_LIST_NO_PARAMETER");
                    }

                    return string.Format(localizedListType, GetLocalizedType(listElementType));
                }
            }

            return type.Name;
        }
    }
}