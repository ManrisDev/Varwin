namespace Varwin.Events
{
    /// <summary>
    /// Пустой интерфейс для маркировки событий.
    /// </summary>
    public interface IEvent{}

    /// <summary>
    /// Базовый интерфейс для слушателей событий.
    /// </summary>
    public interface IBaseEventReceiver { }

    public interface IEventReceiver<in T> : IBaseEventReceiver where T : struct, IEvent
    {
        public void OnEvent(T eventArg);
    }
}