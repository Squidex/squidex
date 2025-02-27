﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent;

public class JsonFieldTests : IClassFixture<TranslationsFixture>
{
    private readonly List<string> errors = [];

    [Fact]
    public void Should_instantiate_field()
    {
        var sut = Field(new JsonFieldProperties());

        Assert.Equal("myJson", sut.Name);
    }

    [Fact]
    public async Task Should_not_add_error_if_json_is_valid()
    {
        var sut = Field(new JsonFieldProperties());

        await sut.ValidateAsync(CreateValue(JsonValue.Create(1)), errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_add_error_if_json_is_required()
    {
        var sut = Field(new JsonFieldProperties { IsRequired = true });

        await sut.ValidateAsync(CreateValue(JsonValue.Null), errors);

        errors.Should().BeEquivalentTo(
            ["Field is required."]);
    }

    private static JsonValue CreateValue(JsonValue v)
    {
        return v;
    }

    private static RootField<JsonFieldProperties> Field(JsonFieldProperties properties)
    {
        return Fields.Json(1, "myJson", Partitioning.Invariant, properties);
    }
}
