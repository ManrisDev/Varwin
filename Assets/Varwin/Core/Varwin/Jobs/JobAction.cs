using System;

namespace Varwin.Jobs
{
    /// <summary>
    /// Работа, описывающая Action.
    /// </summary>
    public class JobAction : IJob
    {
        /// <summary>
        /// Экземпляр Action.
        /// </summary>
        private Action _action;

        /// <summary>
        /// Инициализация задачи.
        /// </summary>
        /// <param name="action">Экземпляр Action</param>
        public JobAction(Action action) => _action = action;

        /// <summary>
        /// <inheritdoc cref="IJob.Execute"/>
        /// </summary>
        public void Execute() => _action?.Invoke();
    }
}