using System.Reflection;
using SmartLocalization;

namespace Varwin.Log.ErrorsValueObjects
{
    /// <summary>
    /// Объект ошибки валидации типа при присвоении значения сеттера.
    /// </summary>
    public readonly struct SetterValidationError
    {
        #region attributes

        private readonly string _errorMessage;

        #endregion

        #region constructors

        public SetterValidationError(object sender, dynamic wrongValue, PropertyInfo propertyInfo)
        {
            var localizedPropertyName = propertyInfo.GetCustomAttribute<VariableAttribute>()?.LocalizedNames?.GetCurrentLocale() ?? propertyInfo.Name;

            if (localizedPropertyName == I18nEx.GetLocaleErrorMessage)
            {
                localizedPropertyName = propertyInfo.Name;
            }

            var targetType = propertyInfo.GetCustomAttribute<SourceTypeContainerAttribute>()?.TargetType ?? propertyInfo.PropertyType;

            string localizedTargetType = LocalizationUtils.GetLocalizedType(targetType);
            string localizedWrongType = string.Empty;

            if (wrongValue is Wrapper valueWrapper)
            {
                localizedWrongType = valueWrapper.GetObjectController().GetLocalizedName();
            }
            else if (string.IsNullOrEmpty(localizedWrongType))
            {
                localizedWrongType = LocalizationUtils.GetLocalizedType(wrongValue?.GetType());
            }

            TypeValidationUtils.TryGetLocalizedObjectName(sender, out string localizedObjectName);

            var blockPrefix = LocalizationUtils.GetLocalizedBlockPrefix(typeof(SetterAttribute));
            var block = $"{blockPrefix} {localizedObjectName} {localizedPropertyName} = {localizedTargetType} ";

            var errorMessage = string.Empty;
            if (LanguageManager.Instance)
            {
                errorMessage = string.Format(LanguageManager.Instance.GetTextValue("ERRORS_ERROR_IN_SETTER"),
                    block,
                    localizedTargetType,
                    localizedWrongType
                );
            }

            _errorMessage = errorMessage;
        }

        #endregion

        #region public methods

        public override string ToString() => _errorMessage;

        #endregion
    }
}