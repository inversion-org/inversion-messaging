using System;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Inversion.Data;
using Inversion.Messaging.Logging;
using Inversion.Process;
using Inversion.Process.Behaviour;
using Newtonsoft.Json.Linq;

namespace Inversion.Messaging.Process.Context
{
    public class TimedLoggingContext : TimedContext
    {
        private ILoggingStore _logger;

        public TimedLoggingContext(IServiceContainer services, IResourceAdapter resources, ILoggingStore logger)
            : base(services, resources)
        {
            _logger = logger;
        }

        public override IEvent Fire(IEvent ev)
        {
            this.LogDebug("fire", ev.Message);
            return base.Fire(ev);
        }

        public override void Register(IProcessBehaviour behaviour)
        {
            this.Bus.Where(behaviour.Condition).Subscribe(
                (IEvent ev) => {
                        string behaviourName = behaviour.GetType().FullName;
                        try
                        {
                            this.LogDebug("action", behaviourName);

                            ev.Context.Timers.Begin(behaviourName);

                            behaviour.Action(ev);

                            ev.Context.Timers.End(behaviourName);
                        }
                        catch (Exception err)
                        {
                            this.Log("rescue", String.Format("{0}: {1}", behaviourName, err.ToString()));
                            behaviour.Rescue(ev, err);

                            StringBuilder actionLog = new StringBuilder();

                            actionLog.AppendLine(String.Format("{0}: {1}", behaviourName, ev.Message));
                            if (behaviour is IPrototypedBehaviour)
                            {
                                actionLog.AppendLine(ToDiagnosticString(((IPrototypedBehaviour)behaviour).Configuration));
                            }
                            actionLog.AppendLine("event:");
                            actionLog.AppendLine(String.Join(",\r\n", ev.Params.Keys.Select(k => String.Format("{0} : {1}", k, ev.Params[k]))));
                            actionLog.AppendLine("params:");
                            actionLog.AppendLine(ev.Context.Params.ToJsonObject().ToString());
                            actionLog.AppendLine("control state:");
                            actionLog.AppendLine(ev.Context.ControlState.ToJsonObject().ToString());

                            this.Log("error", actionLog.ToString());

                            throw;
                        }
                });
        }

        protected void Log(string entity, string message)
        {
            if (_logger != null)
            {
                _logger.Log(entity, message);
            }
        }

        protected void LogDebug(string entity, string message)
        {
            if (_logger != null)
            {
                _logger.LogDebug(entity, message);
            }
        }

        public static string ToDiagnosticString(IConfiguration self)
        {
            StringBuilder sb = new StringBuilder();
            foreach (IConfigurationElement element in self.Elements)
            {
                sb.AppendLine(String.Format("f:{0} s:{1} n:{2} v:{3}", element.Frame, element.Slot, element.Name,
                    element.Value));
            }
            return sb.ToString();
        }
    }
}