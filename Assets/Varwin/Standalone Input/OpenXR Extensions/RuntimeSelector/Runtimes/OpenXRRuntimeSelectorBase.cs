using System.Collections.Generic;

namespace OpenXR.Extensions
{
    /// <summary>
    /// Базовый класс механизма работы с OpenXR подсистемами.
    /// </summary>
    public abstract class OpenXRRuntimeSelectorBase
    {
        /// <summary>
        /// Функция, возвращающая текущий рантайм.
        /// </summary>
        /// <returns>Текущий рантайм.</returns>
        public abstract OpenXRRuntimeBase GetActiveRuntime();

        /// <summary>
        /// Функция, возвращающая список доступных рантаймов. 
        /// </summary>
        /// <returns>Список доступных рантаймов.</returns>
        public abstract IEnumerable<OpenXRRuntimeBase> GetAvailableRuntimes();
        
        /// <summary>
        /// Метод, позволяющий указать активный рантайм.
        /// </summary>
        /// <param name="runtime">Рантайм.</param>
        public abstract void SetActiveRuntime(OpenXRRuntimeBase runtime);
        
    }
}