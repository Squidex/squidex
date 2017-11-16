// ==========================================================================
//  XmlRepositoryGrain.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Runtime;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Users.DataProtection.Orleans.Grains.Implementations
{
    public sealed class XmlRepositoryGrain : GrainV2<Dictionary<string, string>>, IXmlRepositoryGrain
    {
        private readonly ISemanticLog log;

        public XmlRepositoryGrain(IGrainRuntime runtime, ISemanticLog log)
            : base(runtime)
        {
            this.log = log;
        }

        protected override async Task ReadStateAsync()
        {
            try
            {
                await base.ReadStateAsync();
            }
            catch (Exception ex)
            {
                State = new Dictionary<string, string>();

                log.LogError(ex, w => w.WriteProperty("action", "LoadXmlRepository"));
            }
        }

        public Task<string[]> GetAllElementsAsync()
        {
            return Task.FromResult(State.Values.ToArray());
        }

        public Task StoreElementAsync(string element, string friendlyName)
        {
            State[friendlyName] = element;

            return WriteStateAsync();
        }
    }
}
