// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Extensions.Actions;
using Squidex.Extensions.Actions.Script;
using Squidex.Extensions.Actions.Webhook;
using Squidex.Flows;
using Squidex.Flows.Internal;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Rules;

public class RuleEventMigratorTests
{
    private readonly TypeRegistry typeRegistrySerializer = new TypeRegistry();
    private readonly TypeRegistry typeRegistryMigrator = new TypeRegistry();
    private readonly IJsonSerializer serializer;
    private readonly RuleEventMigrator sut;

    public RuleEventMigratorTests()
    {
        typeRegistryMigrator.Add<IEvent, RuleCreated>("RuleCreated");
        typeRegistryMigrator.Add<IEvent, RuleUpdated>("RuleUpdated");
        typeRegistryMigrator.Add<FlowStep, ScriptFlowStep>("Script");

        typeRegistrySerializer.Add<FlowStep, ScriptFlowStep>("Script");
        typeRegistrySerializer.Add<FlowStep, WebhookFlowStep>("Webhook");

        // Create the serializer after the types have been registered.
        serializer = TestUtils.CreateSerializer(typeRegistrySerializer);

        sut = new RuleEventMigrator(typeRegistryMigrator, serializer);
    }

    [Fact]
    public void Should_not_migrate_rule_created_event_if_flow_step_is_known()
    {
        typeRegistryMigrator.Add<FlowStep, WebhookFlowStep>("Webhook");

        var @event = new RuleCreated
        {
            Flow = new FlowDefinition
            {
                Steps = new Dictionary<Guid, FlowStepDefinition>
                {
                    [Guid.Empty] = new FlowStepDefinition
                    {
                        Step = new WebhookFlowStep(),
                    },
                },
            },
        };

        var json = serializer.Serialize(@event, true);

        var result = sut.MigrateEvent(typeRegistryMigrator.GetName<IEvent>(@event.GetType()), json);

        Assert.Null(result);
    }

    [Fact]
    public void Should_migrate_rule_created_event()
    {
        var @event = new RuleCreated
        {
            Flow = new FlowDefinition
            {
                Steps = new Dictionary<Guid, FlowStepDefinition>
                {
                    [Guid.Empty] = new FlowStepDefinition
                    {
                        Step = new WebhookFlowStep(),
                    },
                },
            },
        };

        var json = serializer.Serialize(@event, true);

        var resultJson = sut.MigrateEvent(typeRegistryMigrator.GetName<IEvent>(@event.GetType()), json);
        var resultEvent = serializer.Deserialize<RuleCreated>(resultJson!);

        resultEvent.Should().BeEquivalentTo(
            new RuleCreated
            {
                Flow = new FlowDefinition
                {
                    Steps = new Dictionary<Guid, FlowStepDefinition>
                    {
                        [Guid.Empty] = new FlowStepDefinition
                        {
                            Step = new WebhookFlowStep(),
                        },
                    },
                },
            },
            options => options.RespectingDeclaredTypes());
    }

    [Fact]
    public void Should_not_migrate_rule_updated_event_if_flow_step_is_known()
    {
        typeRegistryMigrator.Add<FlowStep, WebhookFlowStep>("Webhook");

        var @event = new RuleUpdated
        {
            Flow = new FlowDefinition
            {
                Steps = new Dictionary<Guid, FlowStepDefinition>
                {
                    [Guid.Empty] = new FlowStepDefinition
                    {
                        Step = new WebhookFlowStep(),
                    },
                },
            },
        };

        var json = serializer.Serialize(@event, true);

        var result = sut.MigrateEvent(typeRegistryMigrator.GetName<IEvent>(@event.GetType()), json);

        Assert.Null(result);
    }

    [Fact]
    public void Should_migrate_rule_updated_event()
    {
        var @event = new RuleUpdated
        {
            Flow = new FlowDefinition
            {
                Steps = new Dictionary<Guid, FlowStepDefinition>
                {
                    [Guid.Empty] = new FlowStepDefinition
                    {
                        Step = new WebhookFlowStep(),
                    },
                },
            },
        };

        var json = serializer.Serialize(@event, true);

        var resultJson = sut.MigrateEvent(typeRegistryMigrator.GetName<IEvent>(@event.GetType()), json);
        var resultEvent = serializer.Deserialize<RuleCreated>(resultJson!);

        resultEvent.Should().BeEquivalentTo(
            new RuleUpdated
            {
                Flow = new FlowDefinition
                {
                    Steps = new Dictionary<Guid, FlowStepDefinition>
                    {
                        [Guid.Empty] = new FlowStepDefinition
                        {
                            Step = new ScriptFlowStep(),
                        },
                    },
                },
            },
            options => options.RespectingDeclaredTypes());
    }
}
