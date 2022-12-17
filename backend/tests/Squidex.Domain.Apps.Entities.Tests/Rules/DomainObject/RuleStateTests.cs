// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject;

public class RuleStateTests
{
    private readonly IJsonSerializer serializer = TestUtils.CreateSerializer(options =>
    {
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

    [RuleAction]
    public sealed record WebhookAction : RuleAction
    {
        public Uri Url { get; set; }
    }

    static RuleStateTests()
    {
        TestUtils.TypeRegistry.Map(new RuleTypeProvider().Add<WebhookAction>());
    }

    [Fact]
    public void Should_deserialize_state()
    {
        var json = File.ReadAllText("Rules/DomainObject/RuleState.json");

        var deserialized = serializer.Deserialize<RuleDomainObject.State>(json);

        Assert.NotNull(deserialized);
    }

    [Fact]
    public void Should_deserialize_state_from_old_representation()
    {
        var json = File.ReadAllText("Rules/DomainObject/RuleState_Old.json");

        var deserialized = serializer.Deserialize<RuleDomainObject.State>(json);

        Assert.NotNull(deserialized);
    }

    [Fact]
    public void Should_serialize_deserialize_state()
    {
        var json = File.ReadAllText("Rules/DomainObject/RuleState.json").CleanJson();

        var serialized = serializer.Serialize(serializer.Deserialize<RuleDomainObject.State>(json), true);

        Assert.Equal(json, serialized);
    }
}
