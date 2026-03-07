namespace MightyHeroes.Core.EventSystem
{
    /// <summary>
    /// Base class for all event groups.
    /// Holds a shared reference to the event registry.
    /// </summary>
    public abstract class EventGroupBase
    {
        // 1) FIELDS

        protected readonly IEventRegistry Registry;

        // 3) CONSTRUCTOR

        protected EventGroupBase(IEventRegistry registry)
        {
            Registry = registry;
        }
    }
}