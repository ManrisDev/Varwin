using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Varwin.Core.Behaviours;
using Varwin.Public;

namespace Varwin.Editor
{
    public class BlocklyBuilder : MonoBehaviour
    {
        private static BlocklyConfig _config;
        private static Type _wrapperType;
        private static string _blockColor;

        private static Regex _nullableRegex = new Regex(@"System\.Nullable`1\[\[([A-Za-z_.?0-9]+?), .*?\]\]");
        private static Regex _dictionaryRegex = new Regex(@"([A-Za-z_.?0-9]+?)`2\[\[([A-Za-z_.?0-9]+?),.*?\],\[([A-Za-z_.?0-9]+?),.*?\]\]");
        private static Regex _listRegex = new Regex(@"([A-Za-z_.?0-9]+?)`1\[\[([A-Za-z_.?0-9]+?),.*?\]\]");

        public class MethodLocale
        {
            public MethodInfo MemberInfo;
            public LocaleAttribute[] LocaleAttributies;
        }

        public static string CreateBlocklyConfig(Type type, ObjectBuildDescription objectBuild)
        {
            if (!Initialize(type, objectBuild))
            {
                return null;
            }

            AddCustomEnumValueListsToConfig(objectBuild.ContainedObjectDescriptor);
            AddCustomValueListsToConfig(objectBuild);
            AddPropertiesToConfig();
            AddFieldsToConfig();
            AddGenericValueListsToConfig(type);
            AddMethodsToConfig();
            AddEventsToConfig();

            var jsonSerializerSettings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};
            string jsonConfig = JsonConvert.SerializeObject(_config, Formatting.None, jsonSerializerSettings);

            return jsonConfig;
        }

        private static bool Initialize(Type type, ObjectBuildDescription objectBuild)
        {
            if (string.IsNullOrWhiteSpace(type.FullName))
            {
                return false;
            }

            _wrapperType = type;
            VarwinObjectDescriptor varwinObjectDescriptor = objectBuild.ContainedObjectDescriptor;
            var customColorComponent = varwinObjectDescriptor.GetComponent<CustomBlocklyColorScheme>();
            if (customColorComponent)
            {
                Color blockColor = customColorComponent.color;
                _blockColor = $"#{ColorUtility.ToHtmlStringRGB(blockColor).ToLower()}";
            }

            string builtAt = $"{DateTimeOffset.UtcNow:s}Z";

            if (DateTimeOffset.TryParse(varwinObjectDescriptor.BuiltAt, out DateTimeOffset builtAtDateTimeOffset))
            {
                builtAt = $"{builtAtDateTimeOffset.UtcDateTime:s}Z";
            }

            _config = new()
            {
                Guid = varwinObjectDescriptor.Guid,
                RootGuid = varwinObjectDescriptor.RootGuid,
                Locked = varwinObjectDescriptor.Locked,
                Embedded = varwinObjectDescriptor.Embedded,
                
                MobileReady = SdkSettings.Features.Mobile && varwinObjectDescriptor.MobileReady,
                LinuxReady = SdkSettings.Features.Linux && BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64),
                SourcesIncluded = varwinObjectDescriptor.SourcesIncluded,
                DisableSceneLogic = varwinObjectDescriptor.DisableSceneLogic,
                Config = new Config
                {
                    type = $"{varwinObjectDescriptor.Name}_{varwinObjectDescriptor.RootGuid.Replace("-", "")}.{varwinObjectDescriptor.Name}Wrapper",
                    blocks = new List<Block>(),
                },
                Author = new JsonAuthor
                {
                    Name = varwinObjectDescriptor.AuthorName,
                    Email = varwinObjectDescriptor.AuthorEmail,
                    Url = varwinObjectDescriptor.AuthorUrl,
                },
                BuiltAt = $"{builtAt}",
                License = new JsonLicense
                {
                    Code = varwinObjectDescriptor.LicenseCode,
                    Version = varwinObjectDescriptor.LicenseVersion,
                },
                SdkVersion = VarwinVersionInfo.VersionNumber,
                UnityVersion = Application.unityVersion,
                Changelog = new()
                {
                    en = SdkSettings.Features.Changelog ? varwinObjectDescriptor.Changelog : string.Empty,
                    ru = SdkSettings.Features.Changelog ? varwinObjectDescriptor.Changelog : string.Empty
                }
            };

            if (File.Exists(objectBuild.TagsPath))
            {
                var tags = File.ReadAllLines(objectBuild.TagsPath);
                _config.Tags = tags.ToList();
            }

            if (_config.Name == null)
            {
                _config.Name = new I18n();
            }
            
            if (varwinObjectDescriptor.DisplayNames != null && varwinObjectDescriptor.DisplayNames.Count > 0)
            {
                _config.Name = varwinObjectDescriptor.DisplayNames.ToI18N();
            }
            else
            {
                IWrapperAware iWrapperAware = varwinObjectDescriptor.gameObject.GetComponent<VarwinObject>();

                if (iWrapperAware != null)
                {
                    _config.Name = LocalizationUtils.GetI18n(iWrapperAware.GetType());
                }

                if (_config.Name == null || _config.Name.IsEmpty())
                {
                    iWrapperAware = varwinObjectDescriptor.gameObject.GetComponents<IWrapperAware>().FirstOrDefault(x => !(x is VarwinObjectDescriptor || x is VarwinObject));
                    _config.Name = iWrapperAware != null ? LocalizationUtils.GetI18n(iWrapperAware.GetType()) : new I18n {en = varwinObjectDescriptor.Name};
                }
            }
            
            if (_config.Name.IsEmpty())
            {
                throw new Exception($"{objectBuild.ObjectName} does not have a localized name.");
            }

            if (_config.Description == null)
            {
                _config.Description = new I18n();
            }
            
            if (varwinObjectDescriptor.Description != null && varwinObjectDescriptor.Description.Count > 0)
            {
                _config.Description = varwinObjectDescriptor.Description.ToI18N();
            }
            else
            {
                _config.Description = _config.Name;
            }
            
            return true;
        }

        private static void AddPropertiesToConfig()
        {
            foreach (PropertyInfo property in _wrapperType.GetProperties())
            {
                var attributes = property.GetCustomAttributes(true);
                var argsParamAttribute = property.GetCustomAttribute<ArgsFormatAttribute>();
                var locales = property.GetCustomAttributes<LocaleAttribute>(true);
                bool ignoreGetter = property.GetCustomAttribute<IgnoreLogicEditorGetterAttribute>() != null;

                string blockName = GetBlockGroupName(attributes);
                VariableAttribute variable = null;

                foreach (object attribute in attributes)
                {
                    Block block = null;
                    Block secondBlock = null;
                    var item = new Item {property = property.Name};

                    switch (attribute)
                    {
                        case GetterAttribute getter:

                            if (ignoreGetter)
                            {
                                break;
                            }

                            block = GetBlock(getter.Name, "getter");
                            block.isObsolete = property.GetCustomAttribute<ObsoleteAttribute>() != null;
                            break;
                        
                        case SetterAttribute setter:
                            block = GetBlock(setter.Name, "setter");
                            block.isObsolete = property.GetCustomAttribute<ObsoleteAttribute>() != null;
                            
                            if (argsParamAttribute != null)
                            {
                                block.valueArgFormat = argsParamAttribute.LocalizedFormat;
                            }
                            break;
                        
                        case VariableAttribute var:

                            if (property.GetMethod != null && !ignoreGetter)
                            {
                                block = blockName == null ? GetBlock(property.Name + "Getter", "getter") : GetBlock(blockName + "Getter", "getter");
                                block.isObsolete = property.GetCustomAttribute<ObsoleteAttribute>() != null;
                            }

                            if (property.SetMethod != null)
                            {
                                secondBlock = blockName == null ? GetBlock(property.Name + "Setter", "setter") : GetBlock(blockName + "Setter", "setter");
                                secondBlock.isObsolete = property.GetCustomAttribute<ObsoleteAttribute>() != null;

                                if (argsParamAttribute != null)
                                {
                                    secondBlock.valueArgFormat = argsParamAttribute.LocalizedFormat;
                                }
                            }
                            
                            variable = var;
                            break;

                        default:
                            continue;
                    }

                    bool isDynamic = property.GetCustomAttributes(typeof(DynamicAttribute), true).Length > 0;
                    var propertyType = property.PropertyType;
                    var sourceTypeAttributes = property.GetCustomAttribute<SourceTypeContainerAttribute>(true);
                    if (sourceTypeAttributes != null)
                    {
                        propertyType = sourceTypeAttributes.TargetType;
                        isDynamic = false;
                    }

                    if (block != null)
                    {
                        if (!propertyType.IsEnum)
                        {
                            block.valueType = GetValidTypeName(propertyType, isDynamic);
                        }
                        
                        var groupAttribute = (LogicGroupAttribute)property.GetCustomAttribute(typeof(LogicGroupAttribute), true);
                        var tooltipAttribute = (LogicTooltipAttribute)property.GetCustomAttribute(typeof(LogicTooltipAttribute), true);
                        block.group = groupAttribute?.LocalizedNames;
                        block.tooltip = tooltipAttribute?.LocalizedNames;
                    }

                    if (secondBlock != null)
                    {
                        if (!propertyType.IsEnum)
                        {
                            secondBlock.valueType = GetValidTypeName(propertyType, isDynamic);
                        }
                        
                        var groupAttribute = (LogicGroupAttribute)property.GetCustomAttribute(typeof(LogicGroupAttribute), true);
                        var tooltipAttribute = (LogicTooltipAttribute)property.GetCustomAttribute(typeof(LogicTooltipAttribute), true);
                        secondBlock.group = groupAttribute?.LocalizedNames;
                        secondBlock.tooltip = tooltipAttribute?.LocalizedNames;
                    }

                    if (isDynamic)
                    {
                        if (block != null)
                        {
                            block.valueType = "dynamic";
                        }

                        if (secondBlock != null)
                        {
                            secondBlock.valueType = "dynamic";
                        }
                    }

                    if (propertyType.IsEnum)
                    {
                        if (block != null)
                        {
                            block.valueType = $"o_{GetBlocklyGuid()}_{propertyType.Name}";
                        }

                        if (secondBlock != null)
                        {
                            secondBlock.valueType =  $"o_{GetBlocklyGuid()}_{propertyType.Name}";
                        }
                    }

                    if (variable != null)
                    {
                        item.i18n = LocalizationUtils.ReplaceEmptyWithWhiteSpace(variable.LocalizedNames);
                    }
                    else
                    {
                        foreach (var locale in locales)
                        {
                            if (locale.I18n != null)
                            {
                                item.i18n = locale.I18n;
                                break;
                            }

                            if (locale.Strings.Length == 1)
                            {
                                item.SetLocale(locale.Code, locale.Strings[0]);
                            }
                            else
                            {
                                Debug.LogError(SdkTexts.PropertyLocaleAttributeMustHaveString);
                            }
                        }
                    }

                    block?.AddItem(item);
                    secondBlock?.AddItem(item);
                }
            }
        }

        private static void AddFieldsToConfig()
        {
            foreach (FieldInfo field in _wrapperType.GetFields())
            {
                var attributes = field.GetCustomAttributes<ValueAttribute>(true);
                var locales = field.GetCustomAttributes<LocaleAttribute>();
                var valueListAttribute = field.GetCustomAttribute<ValueListAttribute>();

                if (valueListAttribute != null)
                {
                    continue;
                }

                foreach (ValueAttribute valueAttribute in attributes)
                {
                    Block block = GetBlock(valueAttribute.Name, "values");
                    var item = new Item {name = field.Name};
                    block.isObsolete = field.GetCustomAttribute<ObsoleteAttribute>() != null;

                    foreach (var locale in locales)
                    {
                        if (locale.I18n != null)
                        {
                            item.i18n = locale.I18n;
                            break;
                        }

                        if (locale.Strings.Length == 1)
                        {
                            item.SetLocale(locale.Code, locale.Strings[0]);
                        }
                        else
                        {
                            Debug.LogError(SdkTexts.PropertyLocaleAttributeMustHaveString);
                        }
                    }

                    block.AddItem(item);
                }
            }
        }

        private static void AddGenericValueListsToConfig(Type type)
        {
            var mainTypefields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var valueListValueDescriptors = WrapperGenerator.GetValueListValueDescriptors(mainTypefields);

            foreach (var valueListValueDescriptor in valueListValueDescriptors)
            {
                Block block = GetBlock(valueListValueDescriptor.Key, "dynamicValueDictionary");
                block.isObsolete = type.GetCustomAttribute<ObsoleteAttribute>() != null;

                block.valueType = GetValueListBlockValueType(valueListValueDescriptor.Key);

                foreach (var valueDescriptor in valueListValueDescriptor.Value)
                {
                    var item = new Item {name = valueDescriptor.FullTypeName, i18n = valueDescriptor.Locale};
                    block.AddItem(item);
                }
            }
        }

        private static void AddCustomEnumValueListsToConfig(VarwinObjectDescriptor varwinObjectDescriptor)
        {
            var componentReferences = varwinObjectDescriptor.Components.ComponentReferences;

            foreach (ComponentReference componentReference in componentReferences)
            {
                if (componentReference.Component is VarwinBehaviour)
                {
                    continue;
                }

                var typeMethods = componentReference.Type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                var typeProperties = componentReference.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var typeEvents = componentReference.Type.GetEvents(BindingFlags.Public | BindingFlags.Instance);

                var enumTypes = WrapperGenerator.ParseMethodsForCustomEnums(typeMethods);
                enumTypes.AddRange(WrapperGenerator.ParsePropertiesForCustomEnums(typeProperties));
                enumTypes.AddRange(WrapperGenerator.ParseEventsForCustomEnums(typeEvents));

                var customEnumValueDescriptors = WrapperGenerator.GetCustomEnumValueDescriptors(enumTypes.ToHashSet().ToList());

                foreach (var customEnumValueList in customEnumValueDescriptors)
                {
                    Block block = GetBlock(customEnumValueList.Key, "dynamicValueDictionary");
                    block.isObsolete = componentReference.Type.GetCustomAttribute<ObsoleteAttribute>() != null;
                    block.valueType = GetValueListBlockValueType(customEnumValueList.Key);
                    var logicGroup = customEnumValueList.Value[0].Group;

                    if (logicGroup != null)
                    {
                        block.group = logicGroup;
                    }

                    foreach (var customEnumValue in customEnumValueList.Value)
                    {
                        if (!block.items.Exists(x => x.name == customEnumValue.FullTypeName))
                        {
                            var item = new Item {name = customEnumValue.FullTypeName, i18n = customEnumValue.Locale};
                            block.AddItem(item);
                        }
                    }
                }
            }
        }

        private static string GetValueListBlockValueType(string valueName) => $"o_{GetBlocklyGuid()}_{valueName.Replace(" ", "")}";

        private static void AddCustomValueListsToConfig(ObjectBuildDescription objectBuild)
        {
            ComponentReference animationsContainerComponentReference = 
                objectBuild.ContainedObjectDescriptor.Components.FirstOrDefault(x => x.Type == typeof(VarwinAnimationPlayer));
            if (animationsContainerComponentReference == null)
            {
                return;
            }

            var customAnimations = new List<VarwinCustomAnimation>();
            var valueListName = "";
            Component component = animationsContainerComponentReference.Component;
            
            if (component is VarwinAnimationPlayer varwinAnimationPlayer)
            {
                customAnimations = varwinAnimationPlayer.GetCustomAnimations();
                valueListName = varwinAnimationPlayer.GetCustomAnimationsValueListName();
                var blockName = $"{animationsContainerComponentReference.PrefixName}{valueListName}";
                Block block = GetBlock(blockName, "dynamicValueDictionary");
                block.valueType = GetValueListBlockValueType(blockName);

                if (customAnimations.Count > 0)
                {
                    for (int i = 0; i < customAnimations.Count; i++)
                    {
                        if (!customAnimations[i].Clip)
                        {
                            continue;
                        }
                        
                        var clipNameSpecified = false;
                        foreach (LocalizationString localizationString in customAnimations[i].Name.LocalizationStrings)
                        {
                            clipNameSpecified = clipNameSpecified || !string.IsNullOrWhiteSpace(localizationString.value);
                        }
                        I18n itemI18n = clipNameSpecified 
                            ? customAnimations[i].Name.ToI18N() 
                            : new I18n {en = ObjectHelper.ConvertToNiceName(customAnimations[i].Clip.name)};

                        var descriptor = objectBuild.ContainedObjectDescriptor;
                        var clipName = customAnimations[i].Clip.name;
                        var enumName = $"Varwin.Types.{descriptor.Namespace}.{descriptor.Name}Wrapper.{WrapperGenerator.AnimationEnumName}.{clipName}{i}";
                        var item =  new Item {name = enumName, i18n = itemI18n};
                        block.AddItem(item);
                    }
                }
                else
                {
                    var item = new Item {name = "no animation", i18n = new I18n {en = "no animation", ru = "нет анимации"}};
                    block.AddItem(item);
                }
            }
        }

        private static void AddMethodsToConfig()
        {
            foreach (MethodInfo method in _wrapperType.GetMethods())
            {
                var attributes = method.GetCustomAttributes(true);
                var locales = method.GetCustomAttributes<LocaleAttribute>().ToArray();
                var argsFormat = method.GetCustomAttribute<ArgsFormatAttribute>();
                var defaultValueAttributes = method.GetCustomAttribute<DefaultValueAttribute>();
                var defaultValueTypeAttributes = method.GetCustomAttribute<DefaultValueTypeAttribute>();

                if (defaultValueAttributes != null && defaultValueAttributes.DefaultValues.Length != method.GetParameters().Length)
                {
                    throw new BlocklyArgumentsException();
                }

                if (defaultValueTypeAttributes != null && defaultValueTypeAttributes.DefaultValueTypes.Length != method.GetParameters().Length)
                {
                    throw new BlocklyArgumentsException();
                }

                var blockGroupName = GetBlockGroupName(attributes);
                var isInActionGroup = blockGroupName != null;

                foreach (object attribute in attributes)
                {
                    Block block;
                    Item item = new Item {method = method.Name};

                    switch (attribute)
                    {
                        case CheckerAttribute checker:
                            if (method.ReturnType != typeof(bool))
                            {
                                Debug.LogError(string.Format(SdkTexts.MethodMustReturnBoolFormat, method.Name));
                                continue;
                            }

                            block = isInActionGroup ? GetBlock(blockGroupName, "checker") : GetBlock(checker.Name ?? method.Name, "checker");
                            item.i18n = checker.LocalizedNames;
                            break;
                        
                        case ActionAttribute action:
                            block = isInActionGroup ? GetBlock(blockGroupName, "action") : GetBlock(action.Name ?? method.Name, "action");
                            var groupAlreadyHasItem = isInActionGroup && block.items.Count > 0;
                            var methodIsCoroutine = method.ReturnType != typeof(void);
                            if (groupAlreadyHasItem && block.isCoroutine != methodIsCoroutine)
                            {
                                EditorUtility.DisplayDialog(SdkTexts.DifferentActionGroupBlockReturnTypeTitle,
                                    string.Format(SdkTexts.DifferentActionGroupBlockReturnTypeMessage, block.name, method.Name), "OK");
                                throw new Exception(string.Format(SdkTexts.DifferentActionGroupBlockReturnTypeMessage, block.name, method.Name));
                            }

                            block.isCoroutine = methodIsCoroutine;
                            item.i18n = action.LocalizedNames;
                            break;
                        
                        case FunctionAttribute function:
                            block = isInActionGroup ? GetBlock(blockGroupName, "function") : GetBlock(method.Name, "function");
                            item.i18n = function.LocalizedNames;
                            break;

                        default:
                            continue;
                    }

                    block.isObsolete = method.GetCustomAttribute<ObsoleteAttribute>() != null;
                    
                    var groupAttribute = (LogicGroupAttribute)method.GetCustomAttribute(typeof(LogicGroupAttribute), true);
                    var tooltipAttribute = (LogicTooltipAttribute)method.GetCustomAttribute(typeof(LogicTooltipAttribute), true);
                    block.group = groupAttribute?.LocalizedNames;
                    block.tooltip = tooltipAttribute?.LocalizedNames;
                    
                    foreach (var locale in locales)
                    {
                        if (locale.I18n != null)
                        {
                            item.i18n = locale.I18n;
                            break;
                        }

                        if (locale.Strings.Length > 0)
                        {
                            item.SetLocale(locale.Code, locale.Strings[0]);
                        }
                    }

                    if (argsFormat != null)
                    {
                        item.variablesFormat = argsFormat.LocalizedFormat;
                    }
                    
                    foreach (var blockItem in block.items)
                    {
                        if (!I18n.Equals(item.variablesFormat, blockItem.variablesFormat))
                        {
                            var methodType = $"{method.DeclaringType}.{method.Name}";

                            var blockName = block.name;
                            if (!string.IsNullOrEmpty(item.method))
                            {
                                blockName += $".{item.method}";
                            }

                            Debug.LogErrorFormat(SdkTexts.ArgsFormatIsNotEqualsFormat, blockName, methodType);
                            throw new BlocklyArgsFormatIsNotEqualsException();
                        }
                    }

                    int i = 1;

                    var parameters = method.GetParameters();

                    if (parameters.Length > 0)
                    {
                        var args = new List<Arg>();

                        for (var index = 0; index < parameters.Length; index++)
                        {
                            ParameterInfo param = parameters[index];
                            
                            bool isDynamic = param.GetCustomAttributes(typeof(DynamicAttribute), true).Length > 0;
                            var parameterType = param.ParameterType;
                            var sourceArgumentAttribute = param.GetCustomAttribute<SourceTypeContainerAttribute>();
                            if (sourceArgumentAttribute != null)
                            {
                                parameterType = sourceArgumentAttribute.TargetType;
                                isDynamic = false;
                            }
                            
                            var isEnum = parameterType.IsEnum;
                            var useValueList = (UseValueListAttribute) param.GetCustomAttributes(typeof(UseValueListAttribute), true).FirstOrDefault();
                            string valueType = "";

                            if (isEnum)
                            {
                                valueType = GetValidDynamicDictionaryType(parameterType.Name);
                            }
                            else if (useValueList != null)
                            {
                                valueType = GetValidDynamicDictionaryType(useValueList.ListNames.FirstOrDefault().Replace(" ", ""));
                            }
                            else
                            {
                                valueType = GetValidTypeName(parameterType, isDynamic);
                            }

                            var arg = new Arg {valueType = valueType};
                            if (defaultValueAttributes != null)
                            {
                                arg.defaultValue = defaultValueAttributes.DefaultValues[index];
                            }
                            if (defaultValueTypeAttributes != null)
                            {
                                arg.defaultValueType = GetValidTypeName(defaultValueTypeAttributes.DefaultValueTypes[index]);
                            }

                            I18n i18n = (param.GetCustomAttribute(typeof(ParameterAttribute), true) as ParameterAttribute)?.LocalizedNames;

                            if (i18n != null)
                            {
                                arg.i18n = i18n;
                            }
                            else
                            {
                                int j = 0;

                                foreach (LocaleAttribute locale in locales)
                                {
                                    if (locale.I18n != null)
                                    {
                                        if (j == i)
                                        {
                                            arg.i18n = locale.I18n;

                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (locale.Strings.Length > i)
                                        {
                                            arg.SetLocale(locale.Code, locale.Strings[i]);
                                        }
                                    }

                                    j++;
                                }
                            }

                            if (isDynamic)
                            {
                                arg.valueType = "dynamic";
                            }

                            args.Add(arg);

                            i++;
                        }

                        if (block.args == null)
                        {
                            block.args = new List<Arg>();
                            block.args.AddRange(args);
                        }
                        else
                        {
                            if (block.args.Count != args.Count)
                            {
                                Debug.LogError(SdkTexts.CountArgumentsMethodsIsNotEquals);

                                throw new BlocklyArgumentsException();
                            }

                            if (args.Where((t, pIndex) => t.valueType != block.args[pIndex].valueType).Any())
                            {
                                Debug.LogError(SdkTexts.CountArgumentsMethodsIsNotEquals);

                                throw new BlocklyArgumentsException();
                            }
                        }
                    }


                    block.AddItem(item);
                }
            }
        }

        private static string GetValidDynamicDictionaryType(string typeName) => $"o_{GetBlocklyGuid()}_{typeName}".Replace("+", ".");

        private static string GetBlocklyGuid() => _config.Guid.ReplaceAtIndex(_config.Guid.IndexOf('-'), '_');

        private static string GetBlockGroupName(object[] attributes)
        {
            foreach (object attribute in attributes)
            {
                if (attribute is GroupAttribute groupAttribute)
                {
                    return groupAttribute.GroupName;
                }
            }

            return null;
        }

        private static void AddEventsToConfig()
        {
            var eventDelegates = new Dictionary<string, MethodLocale>();
            var customSenderParams = new Dictionary<string, Param>();
            var events = _wrapperType.GetEvents();

            var defaultSenderBlocks = new List<Block>();

            foreach (EventInfo eventInfo in events)
            {
                var logicEvents = eventInfo.GetCustomAttributes<LogicEventAttribute>(true).ToHashSet();
                var legacyEvents = eventInfo.GetCustomAttributes<EventAttribute>(true).ToHashSet();

                var allEvents = new HashSet<Attribute>();
                allEvents.AddRange(logicEvents);
                allEvents.AddRange(legacyEvents);

                var locales = eventInfo.GetCustomAttributes<LocaleAttribute>();
                var paramsFormat = eventInfo.GetCustomAttribute<ArgsFormatAttribute>();
                var customAttributes = eventInfo.GetCustomAttributes(true);
                string blockGroup = GetBlockGroupName(customAttributes);

                foreach (var eventAttribute in allEvents)
                {
                    EventAttribute oldEvent = legacyEvents.Contains(eventAttribute) ? eventAttribute as EventAttribute : null;
                    LogicEventAttribute logicEventAttribute = logicEvents.Contains(eventAttribute) ? eventAttribute as LogicEventAttribute : null;

                    var isLegacyEvent = oldEvent != null;

                    string eventName;

                    if (blockGroup != null)
                    {
                        eventName = blockGroup;
                    }
                    else
                    {
                        eventName = isLegacyEvent ? oldEvent.Name ?? eventInfo.Name : eventInfo.Name;
                    }

                    Block block = GetBlock(eventName, isLegacyEvent ? "event" : "eventWithArguments");
                    var item = new Item {method = eventInfo.Name, i18n = isLegacyEvent ? oldEvent.LocalizedNames : logicEventAttribute.LocalizedNames};
                    block.isObsolete = eventInfo.GetCustomAttribute<ObsoleteAttribute>() != null;
                    block.isObsolete |= isLegacyEvent;
                    
                    var groupAttribute = (LogicGroupAttribute)eventInfo.GetCustomAttribute(typeof(LogicGroupAttribute), true);
                    var tooltipAttribute = (LogicTooltipAttribute)eventInfo.GetCustomAttribute(typeof(LogicTooltipAttribute), true);
                    block.group = groupAttribute?.LocalizedNames;
                    block.tooltip = tooltipAttribute?.LocalizedNames;

                    foreach (var locale in locales)
                    {
                        if (locale.I18n != null)
                        {
                            item.i18n = locale.I18n;
                            break;
                        }

                        if (locale.Strings.Length > 0)
                        {
                            item.SetLocale(locale.Code, locale.Strings[0]);
                        }
                    }

                    if (paramsFormat != null)
                    {
                        item.variablesFormat = paramsFormat.LocalizedFormat;
                    }

                    block.items.Add(item);
                    Type delegateType = eventInfo.EventHandlerType;
                    MethodInfo method = delegateType.GetMethod("Invoke");

                    if (eventInfo.GetCustomAttribute<EventCustomSenderAttribute>() is EventCustomSenderAttribute customSenderAttr)
                    {
                        var paramBlockly = new Param
                        {
                            name = "sender",
                            valueType = "Varwin.Wrapper"
                        };
                
                        I18n i18n = customSenderAttr.LocalizedSender;
                
                        if (i18n != null)
                        {
                            paramBlockly.i18n = i18n;
                        }
                        else
                        {
                            paramBlockly.i18n = CreateDefaultSenderLocalization();
                        }

                        string key = $"{blockGroup};{eventInfo.Name}";
                        customSenderParams.Add(key, paramBlockly);
                    }
                    else if(!isLegacyEvent)
                    {
                        defaultSenderBlocks.Add(block);
                    }

                    if (eventDelegates.ContainsKey(eventName))
                    {
                        continue;
                    }

                    if (method == null)
                    {
                        continue;
                    }

                    if (method.DeclaringType == null)
                    {
                        continue;
                    }

                    var localeAttributes = method.DeclaringType.GetCustomAttributes<LocaleAttribute>();

                    var methodLocale = new MethodLocale
                    {
                        MemberInfo = method,
                        LocaleAttributies = localeAttributes.ToArray()
                    };

                    eventDelegates.Add(eventName, methodLocale);
                }
            }

            foreach (var eventDelegate in eventDelegates)
            {
                Block block = GetBlock(eventDelegate.Key);
                int i = 0;

                foreach (ParameterInfo param in eventDelegate.Value.MemberInfo.GetParameters())
                {
                    if (block.@params == null)
                    {
                        block.@params = new List<Param>();
                    }

                    bool isDynamic = param.GetCustomAttributes(typeof(DynamicAttribute), true).Length > 0;

                    var paramBlockly = new Param
                    {
                        name = param.Name,
                        valueType = GetValidTypeName(param.ParameterType, isDynamic)
                    };

                    I18n i18n = (param.GetCustomAttribute(typeof(ParameterAttribute), true) as ParameterAttribute)?.LocalizedNames;

                    if (i18n != null)
                    {
                        paramBlockly.i18n = i18n;
                    }
                    else
                    {
                        int j = 0;

                        foreach (LocaleAttribute locale in eventDelegate.Value.LocaleAttributies)
                        {
                            if (locale.I18n != null)
                            {
                                if (i == j)
                                {
                                    paramBlockly.i18n = locale.I18n;

                                    break;
                                }
                            }
                            else
                            {
                                if (locale.Strings.Length > i)
                                {
                                    paramBlockly.SetLocale(locale.Code, locale.Strings[i]);
                                }
                            }
                            j++;
                        }
                    }

                    if (param.GetCustomAttributes(typeof(DynamicAttribute), true).Length > 0 || param.ParameterType.IsEnum)
                    {
                        paramBlockly.valueType = "dynamic";
                    }

                    block.@params.Add(paramBlockly);

                    i++;
                }
            }

            foreach (var customSenderParam in customSenderParams)
            {
                string groupName = customSenderParam.Key.Split(';')[0];
                string eventName = customSenderParam.Key.Split(';')[1];

                bool hasGroup = !string.IsNullOrEmpty(groupName);
                string blockName =  hasGroup ? groupName : eventName;
                
                if (customSenderParam.Value != null)
                {
                    var block = GetBlock(blockName);

                    if (block.@params == null)
                    {
                        block.@params = new List<Param>();
                    }

                    if (hasGroup)
                    {
                        var customSender = block.@params.FirstOrDefault(x => x.name == "sender");
                        bool blockHasCustomSenderParam = block.@params.Count > 0 && customSender != null;

                        if (blockHasCustomSenderParam)
                        {
                            bool sendersAreEqual = customSenderParam.Value.i18n.en == customSender.i18n.en && customSenderParam.Value.i18n.ru == customSender.i18n.ru;
                            if (!sendersAreEqual)
                            {
                                EditorUtility.DisplayDialog(SdkTexts.EventGroupCustomSenderDifferentTitle,
                                    string.Format(SdkTexts.EventGroupCustomSenderDifferent, _config.Name.en, groupName), "OK", "Cancel");
                                throw new NotSupportedException(SdkTexts.EventGroupCustomSenderDifferent);
                            }
                        }
                        else
                        {
                            block.@params.Add(customSenderParam.Value);
                        }
                    }
                    else
                    {
                        block.@params.Add(customSenderParam.Value);
                    }
                }
            }
            AddDefaultEventSenders(defaultSenderBlocks);
        }


        private static void AddDefaultEventSenders(List<Block> blocks)
        {
            Param defaultSender = new Param
            {
                name = "sender",
                valueType = "Varwin.Wrapper",
                i18n = CreateDefaultSenderLocalization()
            };

            foreach (var block in blocks)
            {
                bool hasGroup = block.items.Count > 1;

                if (block.@params == null)
                {
                    block.@params = new List<Param>();
                }

                if (hasGroup)
                {
                    var currentSender = block.@params.FirstOrDefault(x => x.name == "sender");

                    if (currentSender == null)
                    {
                        block.@params.Add(defaultSender);
                    }
                }
                else
                {
                    block.@params.Add(defaultSender);
                }
            }
        }

        private static I18n CreateDefaultSenderLocalization()
        {
            return new()
            {
                en = _config.Name.en,
                ru = string.IsNullOrWhiteSpace(_config.Name.ru) ? _config.Name.en : _config.Name.ru,
                cn = string.IsNullOrEmpty(_config.Name.cn) ? _config.Name.en : _config.Name.cn,
                ko = string.IsNullOrEmpty(_config.Name.ko) ? _config.Name.en : _config.Name.ko,
                kk = string.IsNullOrWhiteSpace(_config.Name.kk) ? _config.Name.en : _config.Name.kk,
            };
        }

        public static string GetValidTypeName(Type type, bool isDynamic = false)
        {
            string typeName = type.FullName;

            if (_nullableRegex.IsMatch(typeName))
            {
                typeName = _nullableRegex.Replace(typeName, "$1?");
            }

            if (_dictionaryRegex.IsMatch(typeName))
            {
                typeName = _dictionaryRegex.Replace(typeName, "$1<$2,$3>");
            }

            if (_listRegex.IsMatch(typeName))
            {
                typeName = _listRegex.Replace(typeName, "$1<$2>");
            }

            if (isDynamic)
            {
                typeName = Regex.Replace(typeName, @"\bSystem\.Object\b", "dynamic");
            }

            string assemblyName = type.Assembly.GetName().Name;

            if (type.Assembly == _wrapperType.Assembly)
            {
                return typeName.Replace("+", ".");
            }

            var allowedAssemblyNames = new List<string> { "VarwinCore", "UnityEngine", "mscorlib", "System", "Unity" };

            if (!allowedAssemblyNames.Any(assemblyName.StartsWith))
            {
                throw new Exception(string.Format(SdkTexts.CustomTypesForbiddenFormat, typeName));
            }

            typeName = GetCustomTypeValidName(typeName).Replace("+", ".");

            return typeName;
        }

        private static string GetCustomTypeValidName(string name)
        {
            switch (name)
            {
                case "UnityEngine.Vector3":
                    return "Vector3";

                case "UnityEngine.Color":
                    return "Color";

                default: 
                    return name;
            }
        }

        private static Block GetBlock(string blockName, string blockType = null, string valueType = null)
        {
            foreach (var block in _config.Config.blocks)
            {
                if (!string.Equals(block.name, blockName, StringComparison.Ordinal))
                {
                    continue;
                }

                if (blockType == null || block.type == blockType)
                {
                    return block;
                }
                
                EditorUtility.DisplayDialog(SdkTexts.SameLogicGroupDifferentBlockTypesTitle,
                    $"{string.Format(SdkTexts.SameLogicGroupDifferentBlockTypesMessage, blockName)}. See console for details.", "OK");

                var requiredType = blockType == "dynamicValueDictionary" ? "enum" : blockType;
                var foundType = block.type == "dynamicValueDictionary" ? "enum" : block.type;

                throw new Exception(
                    $"{SdkTexts.SameLogicGroupDifferentBlockTypesTitle}: {string.Format(SdkTexts.SameLogicGroupDifferentBlockTypesMessage, blockName)}. " +
                    "\nThere are two blocks with same name and different types:\n" +
                    $"1. {blockName} with type {foundType}\n" +
                    $"2. {blockName} with type {requiredType}\n" +
                    "Change one of the block names to fix the issue.");
            }

            var newBlock = new Block
            {
                colour = _blockColor,
                name = blockName,
                items = new List<Item>(),
                type = blockType,
                valueType = valueType
            };
            _config.Config.blocks.Add(newBlock);

            return newBlock;
        }

        internal class BlocklyArgumentsException : Exception
        {
        }
        
        internal class BlocklyArgsFormatIsNotEqualsException : Exception
        {
        }
    }
}
