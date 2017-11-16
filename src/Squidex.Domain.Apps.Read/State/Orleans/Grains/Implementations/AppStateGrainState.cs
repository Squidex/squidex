// ==========================================================================
//  AppStateGrainState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Rules;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains.Implementations
{
    public sealed partial class AppStateGrainState
    {
        private FieldRegistry registry;

        [JsonProperty]
        public JsonAppEntity App { get; set; }

        [JsonProperty]
        public Dictionary<Guid, JsonRuleEntity> Rules { get; set; }

        [JsonProperty]
        public Dictionary<Guid, JsonSchemaEntity> Schemas { get; set; }

        public void SetRegistry(FieldRegistry registry)
        {
            this.registry = registry;
        }

        public IAppEntity GetApp()
        {
            return App;
        }

        public ISchemaEntity FindSchema(Func<JsonSchemaEntity, bool> filter)
        {
            return Schemas.Values?.FirstOrDefault(filter);
        }

        public List<IRuleEntity> FindRules()
        {
            return Rules.Values?.OfType<IRuleEntity>().ToList() ?? new List<IRuleEntity>();
        }

        public List<ISchemaEntity> FindSchemas(Func<JsonSchemaEntity, bool> filter)
        {
            return Schemas.Values?.Where(filter).OfType<ISchemaEntity>().ToList() ?? new List<ISchemaEntity>();
        }

        public void Reset()
        {
            Rules = new Dictionary<Guid, JsonRuleEntity>();

            Schemas = new Dictionary<Guid, JsonSchemaEntity>();
        }

        public void Apply(Envelope<IEvent> envelope)
        {
            this.DispatchAction(envelope.Payload, envelope.Headers);

            if (App != null)
            {
                App.Etag = Guid.NewGuid().ToString();
            }
        }
    }
}