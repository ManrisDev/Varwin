namespace OpenXR.Extensions
{
    /// <summary>
    /// Описание неизвестной подсистемы.
    /// </summary>
    public class UnknownWindowsRuntime : OpenXRWindowsRuntimeBase
    {
        /// <summary>
        /// Имя подсистемы.
        /// </summary>
        public override string Name => $"Unknown {JsonPath}";

        /// <summary>
        /// Инициализация подсистемы.
        /// </summary>
        /// <param name="jsonPath">Путь до json.</param>
        public UnknownWindowsRuntime(string jsonPath)
        {
            JsonPath = jsonPath;
        }
    }
}