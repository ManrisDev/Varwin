using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;
using Varwin.Core.Behaviours;
using Varwin.Log.ErrorsValueObjects;
using Varwin.Public;

namespace Varwin
{
    /// <summary>
    /// Класс для валидации и касте типов.
    /// </summary>
    public static class TypeValidationUtils
    {
        #region public methods

        /// <summary>
        /// Попытаться сконвертировать динамическую переменную к определенному типу.
        /// </summary>
        /// <param name="value">Значение, которое кастится.</param>
        /// <param name="castToType">Тип, к которому кастится значение.</param>
        /// <param name="result">Приведенное к нужному типу значение.</param>
        /// <returns>Удалось ли скастить значение к требуемому типу.</returns>
        public static bool TryConvertToType(dynamic value, Type castToType, out dynamic result)
        {
            result = null;

            var isStruct = IsTypeStruct(castToType) || (value is not null && IsTypeStruct(value.GetType() as Type));

            if (isStruct)
                return TryHandleStructType(value, castToType, ref result);

            if (!DynamicCast.CanConvertToComparingValue(value))
            {
                return TryHandleReferenceType(value, castToType, ref result);
            }

            return TryHandleValueType(value, castToType, out result);
        }

        /// <summary>
        /// Получить строку с локализованной ошибков валидации типа сеттера.
        /// </summary>
        /// <param name="sender">Объект-источник оишбки.</param>
        /// <param name="wrongValue">Некорректное значение, которое пытались установить в свойство.</param>
        /// <param name="nameOfProperty">Имя свойства.</param>
        /// <returns>Локализованная строка с ошибкой.</returns>
        [UsedImplicitly]
        public static string GetSetterValidateError(object sender, dynamic wrongValue, string nameOfProperty)
        {
            PropertyInfo propertyInfo = sender.GetType().GetProperties().FirstOrDefault(x => x.Name == nameOfProperty);

            if (propertyInfo is null)
            {
                return string.Empty;
            }

            return new SetterValidationError(sender, wrongValue, propertyInfo).ToString();
        }

        /// <summary>
        /// Получить локализованную строку с ошибкой валидации типа аргумента метода.
        /// </summary>
        /// <param name="sender">Объект-источник ошибки.</param>
        /// <param name="wrongValue">Некорректный аргумент.</param>
        /// <param name="methodInfo">Информация о методе, в который пытались прокинуть некорректный аргумент.</param>
        /// <param name="parameterIndex">Индекс параметра, который не прошел валидацию.</param>
        /// <returns>Локализованную строку с ошибкой</returns>
        [UsedImplicitly]
        public static string GetMethodValidateError(object sender, dynamic wrongValue, string methodName, int parameterIndex)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                return string.Empty;
            }

            var methodInfo = sender.GetType().GetMethods().FirstOrDefault(method => method.Name == methodName);

            if (methodInfo == null)
            {
                #if UNITY_EDITOR
                Debug.LogError($"Can't find method {methodName} in {sender.GetType()}");
                #endif
                return string.Empty;
            }

            var parameters = methodInfo.GetParameters();
            if (parameters.Length <= parameterIndex)
            {
                return string.Empty;
            }

            return new MethodArgumentValidationError(sender, wrongValue, methodInfo, parameterIndex).ToString();
        }

        /// <summary>
        /// Провалидировать аргумент метода и сконвертировать исходное значение. Если сконвертировать невозможно, то вывести ошибку.
        /// </summary>
        /// <param name="sender">Объект, метод которого нужно провалидировать.</param>
        /// <param name="value">Исходное значение.</param>
        /// <param name="methodName">Имя метода.</param>
        /// <param name="parameterIndex">Индекс параметра.</param>
        /// <param name="convertedValue">Сконвертированное значение.</param>
        /// <typeparam name="T">Тип, в который нужно сконвертировать исходное значение.</typeparam>
        /// <returns>Удалось ли сконвертировать исходное значение.</returns>
        public static bool ValidateMethodWithLog<T>(object sender, dynamic value, string methodName, int parameterIndex, out T convertedValue)
        {
            convertedValue = default(T);

            if (!Validation.ValidateAndConvert(value, typeof(T), out dynamic result))
            {
                Debug.LogError(GetMethodValidateError(sender, value, methodName, parameterIndex));
                return false;
            }

            convertedValue = (T)result;
            return true;
        }

        /// <summary>
        /// Провалидировать и сконвертировать исходное значение для сеттера.
        /// </summary>
        /// <param name="sender">Объект, сеттер которого нужно провалидировать.</param>
        /// <param name="value">Исходное значение.</param>
        /// <param name="propertyName">Имя свойства.</param>
        /// <param name="convertedValue">Сконвертированное значение.</param>
        /// <typeparam name="T">Тип, в который нужно сконвертировать исходное значение.</typeparam>
        /// <returns>Удалось ли сконвертировать исходное значение.</returns>
        public static bool ValidateSetterWithLog<T>(object sender, dynamic value, string propertyName, out T convertedValue)
        {
            convertedValue = default(T);

            if (!Validation.ValidateAndConvert(value, typeof(T), out dynamic result))
            {
                Debug.LogError(GetSetterValidateError(sender, value, propertyName));
                return false;
            }

            convertedValue = (T)result;
            return true;
        }

        /// <summary>
        /// Метод для получения имени объекта.
        /// </summary>
        /// <param name="object">Объект</param>
        /// <returns></returns>
        public static bool TryGetLocalizedObjectName(object @object, out string localizedObjectName)
        {
            localizedObjectName = string.Empty;
            ObjectController objectController = null;

            if (@object is Wrapper wrapper)
            {
                objectController = wrapper.GetObjectController();
            }
            else if (@object is VarwinBehaviour behaviour)
            {
                objectController = behaviour.Wrapper?.GetObjectController();
            }
            else if (@object is MonoBehaviour monoBehaviour && monoBehaviour.gameObject)
            {
                objectController = monoBehaviour.gameObject.GetWrapper()?.GetObjectController();
            }

            if (objectController != null)
            {
                localizedObjectName = $"({objectController.GetLocalizedName()} {objectController.Name})";
            }

            return !string.IsNullOrEmpty(localizedObjectName);
        }
        #endregion

        #region service methods

        /// <summary>
        /// Обработка значимого (value) типа.
        /// </summary>
        /// <param name="value">Значение, которое кастится.</param>
        /// <param name="castToType">Тип, к которому будет кастится значение.</param>
        /// <param name="result">Значение, приведенное к значимому типу. </param>
        /// <returns>Удалось ли скастить значение к нужному значимому типу.</returns>
        private static bool TryHandleValueType(dynamic value, Type castToType, out dynamic result)
        {
            Type valueType = value.GetType();
            if (castToType.IsAssignableFrom(valueType))
            {
                result = value;
                return true;
            }

            return DynamicCast.TryCastValueToType(value, castToType, out result);
        }

        /// <summary>
        /// Обработка ссылочного (reference) типа.
        /// </summary>
        /// <param name="value">Значение, которое кастится.</param>
        /// <param name="castToType">Тип, к которому будет кастится значение.</param>
        /// <param name="result">Значение, приведенное к ссылочному типу.</param>
        /// <returns>Удалось ли привести значение к нужному ссылочному типу.</returns>
        private static bool TryHandleReferenceType(dynamic value, Type castToType, ref dynamic result)
        {
            if (value is null || castToType is null)
            {
                result = null;
                return false;
            }

            if (DynamicCast.TryConvertReference(value, castToType, out result))
            {
                return true;
            }

            return false;
        }

        private static bool IsTypeStruct(Type type) => type.IsValueType && !type.IsPrimitive && !type.IsEnum;

        private static bool TryHandleStructType(dynamic value, Type castToType, ref dynamic result)
        {
            result = null;
            if (castToType is null)
                return false;

            if (castToType.IsInstanceOfType(value))
            {
                result = value;
                return true;
            }

            try
            {
                result = Convert.ChangeType(value, castToType);
                return result != null;
            }
            catch (Exception e)
            {
                result = null;
                return false;
            }
        }

        #endregion
    }

}