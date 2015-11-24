using System;
using System.Collections.Generic;

using Inversion.Data;
using Inversion.Messaging.Model;

namespace Inversion.Messaging.Process
{

    public interface IEngineOverviewStore : IStore
    {
        EngineOverview GetGlobalStatus(string name);
        EngineOverview Get(string name);
        IEnumerable<EngineOverview> GetAll();
    }
}