using Inversion.Data;
using Inversion.Process;

namespace Inversion.Messaging.Transport
{
    /// <summary>
    /// Expresses facility to peek at the tip
    /// message in a backing store.
    /// </summary>
    public interface IPeek : IStore
    {
        /// <summary>
        /// Peeks at the current event
        /// on the tip of a backing store.
        /// </summary>
        /// <returns></returns>
        IEvent Peek();
    }
}