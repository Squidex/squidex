// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent;

public class UserInfoFieldPropertiesTests : IClassFixture<TranslationsFixture>
{
    private readonly List<string> errors = [];

    [Fact]
    public void Should_instantiate_field()
    {
        var sut = Field(new UserInfoFieldProperties());

        Assert.Equal("myUserInfo", sut.Name);
    }

    [Fact]
    public async Task Should_not_add_error_if_userinfo_is_valid_null()
    {
        var sut = Field(new UserInfoFieldProperties());

        await sut.ValidateAsync(JsonValue.Null, errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_userinfo_is_valid()
    {
        var sut = Field(new UserInfoFieldProperties());

        await sut.ValidateAsync(CreateValue("Key", "Role"), errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_add_error_if_userinfo_is_not_an_object()
    {
        var sut = Field(new UserInfoFieldProperties { IsRequired = true });

        await sut.ValidateAsync(JsonValue.True, errors);

        errors.Should().BeEquivalentTo(
            ["Invalid json type, expected apiKey+role object."]);
    }

    [Fact]
    public async Task Should_add_error_if_userinfo_has_invalid_role()
    {
        var sut = Field(new UserInfoFieldProperties { IsRequired = true });

        await sut.ValidateAsync(CreateValue("Key", null), errors);

        errors.Should().BeEquivalentTo(
            ["Role must be defined."]);
    }

    [Fact]
    public async Task Should_add_error_if_userinfo_has_invalid_apiKey()
    {
        var sut = Field(new UserInfoFieldProperties { IsRequired = true });

        await sut.ValidateAsync(CreateValue(null, "Role"), errors);

        errors.Should().BeEquivalentTo(
            ["ApiKey must be defined."]);
    }

    [Fact]
    public async Task Should_add_error_if_userinfo_is_required()
    {
        var sut = Field(new UserInfoFieldProperties { IsRequired = true });

        await sut.ValidateAsync(JsonValue.Null, errors);

        errors.Should().BeEquivalentTo(
            ["Field is required."]);
    }

    private static JsonValue CreateValue(string? apiKey, string? role)
    {
        return JsonValue.Object().Add("apiKey", apiKey).Add("role", role);
    }

    private static RootField<UserInfoFieldProperties> Field(UserInfoFieldProperties properties)
    {
        return Fields.UserInfo(1, "myUserInfo", Partitioning.Invariant, properties);
    }
}
