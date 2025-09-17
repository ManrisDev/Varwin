using System.Linq;
using System.Reflection;
using SmartLocalization;
using UnityEngine;

namespace Varwin.Log.ErrorsValueObjects
{
    /// <summary>
    /// Объект ошибки валидации аргумента у метода.
    /// </summary>
    public readonly struct MethodArgumentValidationError
    {
        #region attributes

        private readonly string _errorMessage;

        #endregion

        #region constructors

        public MethodArgumentValidationError(object sender, dynamic wrongValue, MethodInfo methodInfo, int parameterIndex)
        {
            var parameters = methodInfo.GetParameters();
            var wrongParameter = parameters[parameterIndex];
            var targetType = wrongParameter.GetCustomAttribute<SourceTypeContainerAttribute>()?.TargetType ?? wrongParameter.ParameterType;

            string localizedTargetType = LocalizationUtils.GetLocalizedType(targetType);
            string localizedWrongType = string.Empty;

            if (wrongValue is Wrapper valueWrapper)
            {
                localizedWrongType = valueWrapper.GetObjectController().GetLocalizedName();
            }

            if (string.IsNullOrEmpty(localizedWrongType))
            {
                localizedWrongType = LocalizationUtils.GetLocalizedType(wrongValue?.GetType());
            }

            TypeValidationUtils.TryGetLocalizedObjectName(sender, out var localizedObjectName);

            string blockName = GetLocalizedMethodName(methodInfo, localizedObjectName, parameterIndex);
            string wrongValueString = (string)wrongValue?.ToString();

            string errorMessage = string.Empty;
            if (LanguageManager.Instance)
            {
                errorMessage = string.Format(LanguageManager.Instance.GetTextValue("ERRORS_ERROR_IN_METHOD"),
                    blockName,
                    wrongParameter.Name,
                    localizedTargetType,
                    localizedWrongType,
                    wrongValueString
                );
            }

            _errorMessage = errorMessage;
        }

        #endregion

        #region public methods

        public override string ToString() => _errorMessage;

        #endregion

        #region service methods

        /// <summary>
        /// Построение имени локализованного ментода.
        /// </summary>
        /// <param name="methodInfo">Метод, на основе которого будет построена локаль.</param>
        /// <param name="wrongParameterIndex">Индекс параметра, не прошедшего валидацию.</param>
        /// <returns>Локализованная строка с именем и локализованными типами и аргументами.</returns>
        private static string GetLocalizedMethodName(
            MethodBase methodInfo,
            string localizedObjectName,
            int wrongParameterIndex
        )
        {
            if (methodInfo is null)
            {
                return string.Empty;
            }

            var methodI18N = methodInfo.GetCustomAttribute<ActionAttribute>()?.LocalizedNames 
                             ?? methodInfo.GetCustomAttribute<FunctionAttribute>().LocalizedNames;

            var attributeType = methodInfo.GetCustomAttribute<ActionAttribute>()?.GetType() ?? methodInfo.GetCustomAttribute<FunctionAttribute>()?.GetType();
            var blockPrefix = LocalizationUtils.GetLocalizedBlockPrefix(attributeType);
            var currentLocale = $"{blockPrefix} ({localizedObjectName}) {methodI18N?.GetCurrentLocale()}";

            string methodName = string.IsNullOrEmpty(currentLocale) || currentLocale == I18nEx.GetLocaleErrorMessage
                ? methodInfo.Name
                : currentLocale;

            var parameters = methodInfo.GetParameters();

            if (parameters.Length <= wrongParameterIndex)
            {
                Debug.LogError($"Can't create method {methodName} exception with parameter index {wrongParameterIndex}");
                return string.Empty;
            }

            string args = string.Empty;

            var argsFormat = methodInfo.GetCustomAttribute<ArgsFormatAttribute>()?.LocalizedFormat;
            var localizedArgs = argsFormat == null ? string.Empty : argsFormat.GetCurrentLocale();
            var splittedArgs = localizedArgs.Split("{%}");

            if (splittedArgs.Length < parameters.Length)
            {
                splittedArgs = parameters.Select(x => string.Empty).ToArray();
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                var localizedType = LocalizationUtils.GetLocalizedType(parameters[i].GetCustomAttribute<SourceTypeContainerAttribute>()?.TargetType ??
                                                                       parameters[i].ParameterType);

                var localizedParameter = $"{parameters[i].Name}: {localizedType}";

                if (i == wrongParameterIndex)
                {
                    args += $"{splittedArgs[i]} <Color=Red>{localizedParameter}</Color>";
                }
                else
                {
                    args += $"{splittedArgs[i]} {localizedParameter}";
                }
            }

            if (splittedArgs.Length <= parameters.Length) return $"{methodName}({args})";
            {
                for (int i = parameters.Length; i < splittedArgs.Length; i++)
                {
                    args += splittedArgs[i];
                }
            }

            return $"{methodName}({args})";
        }

        #endregion
    }
}