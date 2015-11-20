using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inversion.Data;
using Inversion.Messaging.Extensions;
using Inversion.Messaging.Model;

namespace Inversion.Messaging.Process
{
    public class FilesystemEngineController : SyncBaseStore, IEngineController
    {
        private readonly string _baseFolder;
        private readonly string _currentStatusFilename;
        private readonly string _desiredStatusFilename;

        public FilesystemEngineController(string baseFolder) : base()
        {
            _baseFolder = baseFolder;
            _currentStatusFilename = Path.Combine(_baseFolder, "{0}-current-status.txt");
            _desiredStatusFilename = Path.Combine(_baseFolder, "{0}-desired-status.txt");
        }

        public override void Dispose()
        {
            // nothing to do
        }

        public void ReceiveCommand(string name, IEngineCommandReceiver engineCommandReceiver, EngineStatus currentStatus)
        {
            // TODO: modify this to use the passed current status of engine

            EngineControlStatus status = new EngineControlStatus
            {
                CurrentStatus = EngineStatus.Paused,
                DesiredStatus = EngineStatus.Paused,
                Name = name,
                Updated = DateTime.Now
            };

            string currentStatusFilename = String.Format(_currentStatusFilename, name);
            if (File.Exists(currentStatusFilename))
            {
                status.CurrentStatus =
                    (EngineStatus) Enum.Parse(typeof(EngineStatus), File.ReadAllText(currentStatusFilename));
            }

            string desiredStatusFilename = String.Format(_desiredStatusFilename, name);
            if (File.Exists(desiredStatusFilename))
            {
                status.DesiredStatus =
                    (EngineStatus) Enum.Parse(typeof(EngineStatus), File.ReadAllText(desiredStatusFilename));
            }

            if (status.CurrentStatus != status.DesiredStatus ||
                (status.CurrentStatus != EngineStatus.Paused && status.DesiredStatus == EngineStatus.Paused) ||
                (status.CurrentStatus != EngineStatus.Off && status.DesiredStatus == EngineStatus.Off))
            {
                engineCommandReceiver.ProcessControlMessage(status);
            }
        }

        public void UpdateCurrentStatus(string name, EngineStatus currentStatus)
        {
            File.WriteAllText(Path.Combine(_baseFolder, String.Concat(name, "-current-status.txt")), currentStatus.ToString());
        }

        public void UpdateDesiredStatus(string name, EngineStatus desiredStatus)
        {
            throw new NotImplementedException();
        }
        public void ForceStatus(string name, EngineControlStatus status)
        {
            this.UpdateCurrentStatus(name, status.CurrentStatus);
            File.WriteAllText(Path.Combine(_baseFolder, String.Concat(name, "-desired-status.txt")), status.DesiredStatus.ToString());
        }
    }
}