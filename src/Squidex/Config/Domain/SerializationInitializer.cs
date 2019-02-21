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
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Config.Domain
{
    public sealed class SerializationInitializer : IInitializable
    {
        private readonly JsonSerializer jsonSerializer;
        private readonly RuleRegistry ruleRegistry;

        public SerializationInitializer(JsonSerializer jsonSerializer, RuleRegistry ruleRegistry)
        {
            this.jsonSerializer = jsonSerializer;

            this.ruleRegistry = ruleRegistry;
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            BsonJsonConvention.Register(jsonSerializer);

            RuleActionConverter.Mapping = ruleRegistry.Actions.ToDictionary(x => x.Key, x => x.Value.Type);

            return TaskHelper.Done;
        }
    }
}
