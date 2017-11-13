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
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Apps.Utils;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Domain.Apps.Events.Rules.Utils;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Domain.Apps.Events.Schemas.Old;
using Squidex.Domain.Apps.Events.Schemas.Utils;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Rules;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Reflection;

#pragma warning disable CS0612 // Type or member is obsolete

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains.Implementations
{
    public sealed class AppStateGrainState
    {
        [JsonProperty]
        public JsonAppEntity App { get; set; }

        [JsonProperty]
        public Dictionary<Guid, JsonRuleEntity> Rules { get; set; }

        [JsonProperty]
        public Dictionary<Guid, JsonSchemaEntity> Schemas { get; set; }

        public IAppEntity GetApp()
        {
            return App;
        }

        public ISchemaEntity FindSchema(Func<JsonSchemaEntity, bool> filter)
        {
            return Schemas.Values.FirstOrDefault(filter);
        }

        public List<IRuleEntity> FindRules()
        {
            return Rules.Values.OfType<IRuleEntity>().ToList();
        }

        public List<ISchemaEntity> FindSchemas(Func<JsonSchemaEntity, bool> filter)
        {
            return Schemas.Values.Where(filter).OfType<ISchemaEntity>().ToList();
        }

        public void Reset()
        {
            Rules = new Dictionary<Guid, JsonRuleEntity>();

            Schemas = new Dictionary<Guid, JsonSchemaEntity>();
        }

        public void Apply(Envelope<IEvent> envelope, FieldRegistry registry)
        {
            switch (envelope.Payload)
            {
                case AppCreated @event:
                {
                    Reset();

                    App = EntityMapper.Create<JsonAppEntity>(@event, envelope.Headers, a =>
                    {
                        SimpleMapper.Map(@event, a);

                        a.Clients = new AppClients();
                        a.Contributors = new AppContributors();

                        a.LanguagesConfig = LanguagesConfig.Build(Language.EN);
                    });

                    break;
                }

                case AppPlanChanged @event:
                    UpdateApp(envelope, a =>
                    {
                        SimpleMapper.Map(@event, a);
                    });
                    break;

                case AppClientAttached @event:
                    UpdateApp(envelope, a =>
                    {
                        a.Clients.Apply(@event);
                    });
                    break;

                case AppClientRevoked @event:
                    UpdateApp(envelope, a =>
                    {
                        a.Clients.Apply(@event);
                    });
                    break;

                case AppClientRenamed @event:
                    UpdateApp(envelope, a =>
                    {
                        a.Clients.Apply(@event);
                    });
                    break;

                case AppClientUpdated @event:
                    UpdateApp(envelope, a =>
                    {
                        a.Clients.Apply(@event);
                    });
                    break;

                case AppContributorRemoved @event:
                    UpdateApp(envelope, a =>
                    {
                        a.Contributors.Apply(@event);
                    });
                    break;

                case AppContributorAssigned @event:
                    UpdateApp(envelope, a =>
                    {
                        a.Contributors.Apply(@event);
                    });
                    break;

                case AppLanguageAdded @event:
                    UpdateApp(envelope, a =>
                    {
                        a.LanguagesConfig.Apply(@event);
                    });
                    break;

                case AppLanguageRemoved @event:
                    UpdateApp(envelope, a =>
                    {
                        a.LanguagesConfig.Apply(@event);
                    });
                    break;

                case AppLanguageUpdated @event:
                    UpdateApp(envelope, a =>
                    {
                        a.LanguagesConfig.Apply(@event);
                    });
                    break;

                case RuleCreated @event:
                    Rules[@event.RuleId] = EntityMapper.Create<JsonRuleEntity>(@event, envelope.Headers, r =>
                    {
                        r.Rule = RuleEventDispatcher.Create(@event);
                    });
                    break;

                case RuleUpdated @event:
                    UpdateRule(envelope, r =>
                    {
                        r.Rule.Apply(@event);
                    });
                    break;

                case RuleEnabled @event:
                    UpdateRule(envelope, r =>
                    {
                        r.Rule.Apply(@event);
                    });
                    break;

                case RuleDisabled @event:
                    Rules.Remove(@event.RuleId);
                    break;

                case SchemaCreated @event:
                    Schemas[@event.SchemaId.Id] = EntityMapper.Create<JsonSchemaEntity>(@event, envelope.Headers, s =>
                    {
                        s.SchemaDef = SchemaEventDispatcher.Create(@event, registry);

                        SimpleMapper.Map(@event, s);
                    });
                    break;

                case FieldAdded @event:
                    UpdateSchema(envelope, s =>
                    {
                        s.SchemaDef.Apply(@event, registry);
                    });
                    break;

                case FieldDeleted @event:
                    UpdateSchema(envelope, s =>
                    {
                        s.SchemaDef.Apply(@event);
                    });
                    break;

                case FieldLocked @event:
                    UpdateSchema(envelope, s =>
                    {
                        s.SchemaDef.Apply(@event);
                    });
                    break;

                case FieldHidden @event:
                    UpdateSchema(envelope, s =>
                    {
                        s.SchemaDef.Apply(@event);
                    });
                    break;

                case FieldShown @event:
                    UpdateSchema(envelope, s =>
                    {
                        s.SchemaDef.Apply(@event);
                    });
                    break;

                case FieldDisabled @event:
                    UpdateSchema(envelope, s =>
                    {
                        s.SchemaDef.Apply(@event);
                    });
                    break;

                case FieldEnabled @event:
                    UpdateSchema(envelope, s =>
                    {
                        s.SchemaDef.Apply(@event);
                    });
                    break;

                case FieldUpdated @event:
                    UpdateSchema(envelope, s =>
                    {
                        s.SchemaDef.Apply(@event);
                    });
                    break;

                case SchemaFieldsReordered @event:
                    UpdateSchema(envelope, s =>
                    {
                        s.SchemaDef.Apply(@event);
                    });
                    break;

                case SchemaUpdated @event:
                    UpdateSchema(envelope, s =>
                    {
                        s.SchemaDef.Apply(@event);
                    });
                    break;

                case SchemaPublished @event:
                    UpdateSchema(envelope, s =>
                    {
                        s.SchemaDef.Apply(@event);
                    });
                    break;

                case ScriptsConfigured @event:
                    UpdateSchema(envelope, s =>
                    {
                        s.SchemaDef.Apply(@event);
                    });
                    break;

                case SchemaDeleted @event:
                    Schemas.Remove(@event.SchemaId.Id);
                    break;

                case WebhookAdded @event:
                    UpdateSchema(envelope);
                    break;

                case WebhookDeleted @event:
                    UpdateSchema(envelope);
                    break;
            }

            if (App != null)
            {
                App.Etag = Guid.NewGuid().ToString();
            }
        }

        private void UpdateApp(Envelope<IEvent> envelope, Action<JsonAppEntity> updater = null)
        {
            var e = envelope.To<AppEvent>();

            App.Update(e.Payload, e.Headers, updater);
        }

        private void UpdateRule(Envelope<IEvent> envelope, Action<JsonRuleEntity> updater = null)
        {
            var e = envelope.To<RuleEvent>();

            Rules[e.Payload.RuleId].Update(e.Payload, e.Headers, updater);
        }

        private void UpdateSchema(Envelope<IEvent> envelope, Action<JsonSchemaEntity> updater = null)
        {
            var e = envelope.To<SchemaEvent>();

            Schemas[e.Payload.SchemaId.Id].Copy().Update(e.Payload, e.Headers, updater);
        }
    }
}