// ==========================================================================
//  ReadSchemaState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Immutable;
using System.Collections.Immutable;
using Benchmarks.Tests.TestData;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.State.Grains;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Benchmarks.Tests
{
    public class ReadSchemaState : Benchmark
    {
        private IServiceProvider services;
        private MyAppState grain;

        public override void Initialize()
        {
            services = Services.Create();

            grain = services.GetRequiredService<IStateFactory>().GetSynchronizedAsync<MyAppState>("DEFAULT").Result;

            var state = new AppStateGrainState
            {
                App = new JsonAppEntity
                {
                    Id = Guid.NewGuid()
                }
            };

            state.Schemas = ImmutableDictionary<Guid, JsonSchemaEntity>.Empty;

            for (var i = 1; i <= 100; i++)
            {
                var schema = new JsonSchemaEntity
                {
                    Id = Guid.NewGuid(),
                    Created = SystemClock.Instance.GetCurrentInstant(),
                    CreatedBy = new RefToken("user", "1"),
                    LastModified = SystemClock.Instance.GetCurrentInstant(),
                    LastModifiedBy = new RefToken("user", "1"),
                    SchemaDef = new Schema("Name")
                };

                for (var j = 1; j < 30; j++)
                {
                    schema.SchemaDef = schema.SchemaDef.AddField(new StringField(j, j.ToString(), Partitioning.Invariant));
                }

                state.Schemas = state.Schemas.Add(schema.Id, schema);
            }

            state.Rules = ImmutableDictionary<Guid, JsonRuleEntity>.Empty;

            for (var i = 0; i < 100; i++)
            {
                var rule = new JsonRuleEntity
                {
                    Id = Guid.NewGuid(),
                    Created = SystemClock.Instance.GetCurrentInstant(),
                    CreatedBy = new RefToken("user", "1"),
                    LastModified = SystemClock.Instance.GetCurrentInstant(),
                    LastModifiedBy = new RefToken("user", "1"),
                    RuleDef = new Rule(new ContentChangedTrigger(), new WebhookAction())
                };

                state.Rules = state.Rules.Add(rule.Id, rule);
            }

            grain.SetState(state);
            grain.WriteStateAsync().Wait();
        }

        public override long Run()
        {
            for (var i = 0; i < 10; i++)
            {
                grain.ReadStateAsync().Wait();
            }

            return 10;
        }

        public override void Cleanup()
        {
            services.Cleanup();
        }
    }
}
