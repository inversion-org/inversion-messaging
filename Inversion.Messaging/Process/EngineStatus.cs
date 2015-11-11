namespace Inversion.Messaging.Process
{
    /// <summary>
    /// The various states that the engine may advertise itself as being in.
    /// </summary>
    public enum EngineStatus
    {
        Off,
        Starting,
        Working,
        WaitingForSlot,
        Stopping,
        Paused,
        Heartbeat,
        Null
    }
}