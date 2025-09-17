using System;
using System.Collections;
using System.Threading.Tasks;
using SmartLocalization;
using UnityEngine;
using Varwin.Public;

namespace Varwin
{
    /// <summary>
    /// Класс методов, которые позволяют выявить объект, в котором произошла ошибка.
    /// </summary>
    public static class DebugUtils
    {
        /// <summary>
        /// Получение итогового текста ошибки.
        /// </summary>
        /// <param name="exception">Исключение.</param>
        /// <param name="gameObject">Вызвавший исключение объект.</param>
        /// <returns>Текст ошибки.</returns>
        private static string GetErrorMessage(Exception exception, GameObject gameObject)
        {
            var wrapper = gameObject.GetWrapper();
            return $"{(LanguageManager.Instance.GetTextValue("IN_OBJECT_HAS_ERROR"))} '{wrapper?.GetName() ?? gameObject.name}': {exception.Message}\n{exception.StackTrace}";
        }
        
        /// <summary>
        /// Запуск корутины с вызовом конструкции Try-catch.
        /// </summary>
        /// <param name="coroutine">Корутина.</param>
        /// <param name="sender">Объект, у которого вызвана корутина.</param>
        /// <returns>Корутина, которая уже вызывается непосредственно в логике/компоненте.</returns>
        public static IEnumerator StartCoroutineWithTryCatch(IEnumerator coroutine, GameObject sender)
        {
            var op = StartCoroutineAsyncWithTryCatch(coroutine, sender);

            yield return new WaitWhile(() => !op.IsCompleted);
        }

        /// <summary>
        /// Переходная пластина от Enumerator в Async. Нужно для того, чтобы корректно отследить try-catch.
        /// Также, нельзя упаковать yield return в try-catch.
        /// </summary>
        /// <param name="coroutine">Корутина.</param>
        /// <param name="sender">Объект, у которого метод вызван.</param>
        /// <returns>Параметр задачи, который вызван асинхронно.</returns>
        private static async Task StartCoroutineAsyncWithTryCatch(IEnumerator coroutine, GameObject sender)
        {
            try
            {
                await coroutine;
            }
            catch (Exception e)
            {
                Debug.LogError(GetErrorMessage(e, sender), sender);
            }
        }
        
        /// <summary>
        /// Выполнение метода с вызовом конструкции Try-catch.
        /// </summary>
        /// <param name="method">Метод.</param>
        /// <param name="sender">Объект, у которого вызван метод.</param>
        public static void ExecuteActionWithTryCatch(Action method, GameObject sender)
        {
            try
            {
                method.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(GetErrorMessage(e, sender), sender);
            }
        }
 
        /// <summary>
        /// Выполнение функции с вызовом конструкции Try-catch.
        /// </summary>
        /// <param name="func">Функция.</param>
        /// <param name="sender">Объект, у которого вызвана функция.</param>
        /// <returns>Результат выполнения функции.</returns>
        public static T ExecuteFunctionWithTryCatch<T>(Func<T> func, GameObject sender)
        {
            try
            {
                return func.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(GetErrorMessage(e, sender), sender);
            }

            return default;
        }
    }
}