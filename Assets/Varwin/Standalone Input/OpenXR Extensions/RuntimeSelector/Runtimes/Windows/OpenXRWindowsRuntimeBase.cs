using System.IO;

namespace OpenXR.Extensions
{
    /// <summary>
    /// Подсистема Windows.
    /// </summary>
    public abstract class OpenXRWindowsRuntimeBase : OpenXRRuntimeBase
    {
        /// <summary>
        /// Имя подсистемы
        /// </summary>
        public abstract override string Name { get; }

        /// <summary>
        /// Доступна ли.
        /// </summary>
        public override bool IsAvailable => File.Exists(JsonPath);

        /// <summary>
        /// Путь к dll подсистемы.
        /// </summary>
        public string JsonPath { get; protected set; }
    }
}