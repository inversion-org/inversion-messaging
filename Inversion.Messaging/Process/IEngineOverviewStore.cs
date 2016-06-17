using System;
using System.Collections.Generic;

using Inversion.Data;
using Inversion.Messaging.Model;

namespace Inversion.Messaging.Process
{

    public interface IEngineOverviewStore : IStore
    {
        IEnumerable<EngineOverview> GetAll();
        IEnumerable<EngineOverview> GetAllGlobalStatus();
        EngineOverview GetGlobalStatus(string name);
        EngineOverview GetEngine(string name);
        IEnumerable<EngineOverview> GetAllEnginesByControl(string name);
        void Remove(string name);
    }
}