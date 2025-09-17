namespace Varwin
{
    /// <summary>
    /// Интерфейс, описывающий объект-клон.
    /// </summary>
    public interface ICloneTarget
    {
        /// <summary>
        /// При создании экземпляра.
        /// </summary>
        /// <param name="source">Исходный объект.</param>
        public void OnInitialize(Wrapper source);
    }
}