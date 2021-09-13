// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Squidex.Areas.Api.Controllers.Rules.Models;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Hosting;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Config.Domain
{
    public sealed class SerializationInitializer : IInitializable
    {
        private readonly JsonSerializer jsonNetSerializer;
        private readonly IJsonSerializer jsonSerializer;
        private readonly RuleRegistry ruleRegistry;

        public int Order => -1;

        public SerializationInitializer(JsonSerializer jsonNetSerializer, IJsonSerializer jsonSerializer, RuleRegistry ruleRegistry)
        {
            this.jsonNetSerializer = jsonNetSerializer;
            this.jsonSerializer = jsonSerializer;

            this.ruleRegistry = ruleRegistry;
        }

        public Task InitializeAsync(
            CancellationToken ct)
        {
            SetupBson();
            SetupOrleans();
            SetupActions();

            return Task.CompletedTask;
        }

        private void SetupActions()
        {
            RuleActionConverter.Mapping = ruleRegistry.Actions.ToDictionary(x => x.Key, x => x.Value.Type);
        }

        private void SetupBson()
        {
            BsonJsonConvention.Register(jsonNetSerializer);
        }

        private void SetupOrleans()
        {
            J.DefaultSerializer = jsonSerializer;
        }
    }
}
