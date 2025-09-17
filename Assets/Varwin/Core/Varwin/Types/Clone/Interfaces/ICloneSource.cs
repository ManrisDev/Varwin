namespace Varwin
{
    /// <summary>
    /// Интерфейс, описывающий клонируемый объект.
    /// </summary>
    public interface ICloneSource
    {
        /// <summary>
        /// Метод, вызываемый перед созданием клона.
        /// </summary>
        public void BeforeCreatingClone();

        /// <summary>
        /// Метод, вызываемый после созданием клона.
        /// </summary>
        /// <param name="cloneObject">Клон.</param>
        public void AfterCreatingClone(Wrapper cloneObject);

        /// <summary>
        /// Метод, вызываемый при удалении клона.
        /// </summary>
        /// <param name="cloneObject">Клон.</param>
        public void OnCloneDestroyed(Wrapper cloneObject);
    }
}