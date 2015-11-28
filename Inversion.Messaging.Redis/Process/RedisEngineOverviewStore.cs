﻿using System;
using System.Collections.Generic;
using System.Linq;
using Inversion.Data.Redis;
using StackExchange.Redis;

using Inversion.Messaging.Model;

namespace Inversion.Messaging.Process
{
    public class RedisEngineOverviewStore : RedisStore, IEngineOverviewStore
    {
        public RedisEngineOverviewStore(string connections, int databaseNumber) : base(connections, databaseNumber) { }

        public EngineOverview GetGlobalStatus(string name)
        {
            HashEntry[] entries = this.Database.HashGetAll(name);

            return new EngineOverview.Builder
            {
                Name = name,
                Updated = entries.Any(e => e.Name == "updated")
                    ? DateTime.Parse(entries.Single(e => e.Name == "updated").Value)
                    : DateTime.MinValue,
                CurrentStatus = EngineStatus.Null,
                DesiredStatus = (EngineStatus) Convert.ToInt32(entries.Single(e => e.Name == "desired").Value)
            };
        }

        public EngineOverview Get(string name)
        {
            HashEntry[] entries = this.Database.HashGetAll(name);

            return this.ConvertHashEntryArrayToEngineOverview(name, entries);
        }

        public IEnumerable<EngineOverview> GetAll()
        {
            IEnumerable<RedisKey> keys =
                this.ConnectionMultiplexer.GetServer(this.Database.IdentifyEndpoint())
                    .Keys(this.Database.Database, "engine:*");

            return keys.Select(k => this.ConvertHashEntryArrayToEngineOverview(k, this.Database.HashGetAll(k)));
        }

        protected EngineOverview ConvertHashEntryArrayToEngineOverview(string name, HashEntry[] entries)
        {
            return new EngineOverview.Builder
            {
                Id = name.GetHashCode().ToString(),
                Name = name,
                CurrentStatus = (EngineStatus) Convert.ToInt32(entries.Single(e => e.Name == "current").Value),
                DesiredStatus = (EngineStatus) Convert.ToInt32(entries.Single(e => e.Name == "desired").Value),
                Updated = DateTime.Parse(entries.Single(e => e.Name == "updated").Value)
            };
        }
    }
}