// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Schemas.DomainObject;

public class SchemaStateTests
{
    private readonly IJsonSerializer serializer = TestUtils.CreateSerializer(options =>
    {
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

    [Fact]
    public void Should_deserialize_state()
    {
        var json = File.ReadAllText("Schemas/DomainObject/SchemaState.json");

        var deserialized = serializer.Deserialize<SchemaDomainObject.State>(json);

        Assert.NotNull(deserialized);
    }

    [Fact]
    public void Should_serialize_deserialize_state()
    {
        var json = File.ReadAllText("Schemas/DomainObject/SchemaState.json").CleanJson();

        var serialized = serializer.Serialize(serializer.Deserialize<SchemaDomainObject.State>(json), true);

        Assert.Equal(json, serialized);
    }
}
