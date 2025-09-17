namespace OpenXR.Extensions
{
    /// <summary>
    /// Описание OpenXR подсистемы.
    /// </summary>
    public abstract class OpenXRRuntimeBase
    {
        /// <summary>
        /// Имя подсистемы.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Доступна ли.
        /// </summary>
        public virtual bool IsAvailable => true;
    }
}