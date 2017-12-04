// ==========================================================================
//  AppStateGrainState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Rules;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Read.State.Grains
{
    public sealed partial class AppStateGrainState : Cloneable<AppStateGrainState>
    {
        private FieldRegistry registry;

        [JsonProperty]
        public JsonAppEntity App { get; set; }

        [JsonProperty]
        public ImmutableDictionary<Guid, JsonRuleEntity> Rules { get; set; } = ImmutableDictionary<Guid, JsonRuleEntity>.Empty;

        [JsonProperty]
        public ImmutableDictionary<Guid, JsonSchemaEntity> Schemas { get; set; } = ImmutableDictionary<Guid, JsonSchemaEntity>.Empty;

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
            return Schemas?.Values.FirstOrDefault(filter);
        }

        public List<ISchemaEntity> FindSchemas(Func<JsonSchemaEntity, bool> filter)
        {
            return Schemas?.Values.Where(filter).OfType<ISchemaEntity>().ToList() ?? new List<ISchemaEntity>();
        }

        public List<IRuleEntity> FindRules()
        {
            return Rules?.Values.OfType<IRuleEntity>().ToList() ?? new List<IRuleEntity>();
        }

        public AppStateGrainState Apply(Envelope<IEvent> envelope)
        {
            return Clone(c =>
            {
                c.DispatchAction(envelope.Payload, envelope.Headers);

                if (c.App != null)
                {
                    c.App.Etag = Guid.NewGuid().ToString();
                }
            });
        }
    }
}