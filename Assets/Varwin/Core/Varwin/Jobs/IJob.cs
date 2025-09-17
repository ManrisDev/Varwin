namespace Varwin.Jobs
{
    /// <summary>
    /// Интерфейс, описывающий работу.
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// Выполнить работу.
        /// </summary>
        void Execute();
    }
}