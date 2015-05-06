using Inversion.Data;
using Inversion.Process;

namespace Inversion.Messaging.Transport
{
    /// <summary>
    /// Expresses facility to pop event
    /// items from a backing store.
    /// </summary>
    public interface IPop : IStore
    {
        /// <summary>
        /// Pops an event from a backing store.
        /// </summary>
        /// <returns></returns>
        IEvent Pop();
    }
}