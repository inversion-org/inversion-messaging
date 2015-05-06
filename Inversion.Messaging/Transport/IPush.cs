using Inversion.Data;
using Inversion.Process;

namespace Inversion.Messaging.Transport
{
    /// <summary>
    /// Expresses facility to push event items onto
    /// a backing store.
    /// </summary>
    public interface IPush : IStore
    {
        /// <summary>
        /// Pushes the event provided onto a backing
        /// store.
        /// </summary>
        /// <param name="ev">
        /// The event to push onto the backing store.
        /// </param>
        void Push(IEvent ev);
    }
}