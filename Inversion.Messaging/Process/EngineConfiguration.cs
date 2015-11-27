using Inversion.Data;
using Inversion.Process;

namespace Inversion.Messaging.Process
{
    /// <summary>
    /// Configuration holding class.
    /// Working defaults are provided for all configuration parameters.
    /// </summary>
    public class EngineConfiguration
    {
        /// <summary>
        /// How many worker tasks to start within the engine. Default is 4.
        /// </summary>
        public int NumberOfWorkerTasks { get { return _numberOfWorkerTasks; } set { _numberOfWorkerTasks = value; } }
        private int _numberOfWorkerTasks = 4;

        /// <summary>
        /// Minimum time the engine task should yield for in-between pumping events (milliseconds). Default is 100ms.
        /// </summary>
        public int EngineMinimumYieldTime { get { return _engineMinimumYieldTime; } set { _engineMinimumYieldTime = value; } }
        private int _engineMinimumYieldTime = 100;

        /// <summary>
        /// Maximum time the engine task should yield for in-between pumping events (milliseconds). Default is 500ms.
        /// </summary>
        public int EngineMaximumYieldTime { get { return _engineMaximumYieldTime; } set { _engineMaximumYieldTime = value; } }
        private int _engineMaximumYieldTime = 500;

        /// <summary>
        /// Amount of time that the engine task should keep running at minimum yield time if readers are not busy (milliseconds). Default is 2000ms.
        /// </summary>
        public int EngineCooldownTime { get { return _engineCooldownTime; } set { _engineCooldownTime = value; } }
        private int _engineCooldownTime = 2000;

        /// <summary>
        /// How long the control handler should yield for in-between attempting to receive commands (milliseconds). Default is 1000ms.
        /// </summary>
        public int ControlHandlerYieldTime { get { return _controlHandlerYieldTime; } set { _controlHandlerYieldTime = value; } }
        private int _controlHandlerYieldTime = 1000;

        /// <summary>
        /// Name of the control element
        /// </summary>
        public string ControlName { get { return _controlName; } set { _controlName = value; } }
        private string _controlName = "engine";

        /// <summary>
        /// Whether the engine should exit once queue has been emptied (will only be consulted once an event has been processed)
        /// </summary>
        public bool ExitOnEmptyQueue { get { return _exitOnEmptyQueue; } set { _exitOnEmptyQueue = value; } }
        private bool _exitOnEmptyQueue = false;

        /// <summary>
        /// Whether the engine should issue a heartbeat in the Run thread rather than a separate TPL Task.
        /// </summary>
        public bool UseInlineHeartbeat {  get { return _useInlineHeartbeat; } set { _useInlineHeartbeat = value; } }
        private bool _useInlineHeartbeat = false;

        /// <summary>
        /// Set the control handler as being a long running task
        /// </summary>
        public bool HeartbeatIsLongRunningTask { get { return _heartbeatIsLongRunningTask; } set { _heartbeatIsLongRunningTask = value; } }
        private bool _heartbeatIsLongRunningTask = true;
    }
}