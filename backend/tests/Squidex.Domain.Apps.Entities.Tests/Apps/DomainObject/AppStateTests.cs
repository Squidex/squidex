// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject;

public class AppStateTests
{
    private readonly IJsonSerializer serializer = TestUtils.CreateSerializer(options =>
    {
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

    [Fact]
    public void Should_deserialize_state()
    {
        var json = File.ReadAllText("Apps/DomainObject/AppState.json");

        var deserialized = serializer.Deserialize<AppDomainObject.State>(json);

        Assert.NotNull(deserialized);
    }

    [Fact]
    public void Should_serialize_deserialize_state()
    {
        var json = File.ReadAllText("Apps/DomainObject/AppState.json").CleanJson();

        var serialized = serializer.Serialize(serializer.Deserialize<AppDomainObject.State>(json), true);

        Assert.Equal(json, serialized);
    }
}
