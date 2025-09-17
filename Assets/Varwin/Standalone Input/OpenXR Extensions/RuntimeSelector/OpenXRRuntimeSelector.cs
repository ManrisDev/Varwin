using System.Collections.Generic;
using System.Linq;

namespace OpenXR.Extensions
{
    /// <summary>
    /// Механизм выбора подсистемы.
    /// </summary>
    public static class OpenXRRuntimeSelector
    {
        /// <summary>
        /// Механизм выбора.
        /// </summary>
        private static OpenXRRuntimeSelectorBase _selectorBase = null;
        
        /// <summary>
        /// Доступно ли переключение.
        /// </summary>
        /// <returns>Истина, если доступна.</returns>
        public static bool IsAvailable()
        {
            return _selectorBase != null && _selectorBase.GetAvailableRuntimes().Any();
        }

        /// <summary>
        /// Ленивая инициализация.
        /// </summary>
        static OpenXRRuntimeSelector()
        {
#if UNITY_STANDALONE_WIN
            _selectorBase = new OpenXRWindowsRuntimeSelector();
#endif
        }

        /// <summary>
        /// Функция, возвращающая текущий рантайм.
        /// </summary>
        /// <returns>Текущий рантайм.</returns>
        public static OpenXRRuntimeBase GetActiveRuntime()
        {
            return _selectorBase.GetActiveRuntime();
        }

        /// <summary>
        /// Функция, возвращающая список доступных рантаймов. 
        /// </summary>
        /// <returns>Список доступных рантаймов.</returns>
        public static IEnumerable<OpenXRRuntimeBase> GetAvailableRuntimes()
        {
            return _selectorBase.GetAvailableRuntimes();
        }

        /// <summary>
        /// Метод, позволяющий указать активный рантайм.
        /// </summary>
        /// <param name="runtime">Рантайм.</param>
        public static void SetActiveRuntime(OpenXRRuntimeBase runtime)
        {
            _selectorBase.SetActiveRuntime(runtime);
        }
    }
}