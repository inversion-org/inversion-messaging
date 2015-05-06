namespace Inversion.Messaging.Transport
{
    /// <summary>
    /// Expresses facility to act as a transport with the
    /// ability to push, pop and peek against a backing
    /// store.
    /// </summary>
    public interface ITransport : IPush, IPop, IPeek
    {
        /// <summary>
        /// The sum count of items on the transport.
        /// </summary>
        /// <returns></returns>
        long Count();
    }
}