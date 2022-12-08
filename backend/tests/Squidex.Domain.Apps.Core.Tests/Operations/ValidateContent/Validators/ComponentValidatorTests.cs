// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent.Validators;

public class ComponentValidatorTests : IClassFixture<TranslationsFixture>
{
    private readonly List<string> errors = new List<string>();

    [Fact]
    public async Task Should_create_validator_from_component_and_invoke()
    {
        var validator = A.Fake<IValidator>();

        var componentData = new JsonObject();
        var componentObject = new Component("type", componentData, new Schema("my-schema"));

        var isFactoryCalled = false;

        var sut = new ComponentValidator(_ =>
        {
            isFactoryCalled = true;
            return validator;
        });

        await sut.ValidateAsync(componentObject, errors);

        Assert.True(isFactoryCalled);

        A.CallTo(() => validator.Validate(componentData, A<ValidationContext>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_do_nothing_if_value_is_not_a_component()
    {
        var isFactoryCalled = false;

        var sut = new ComponentValidator(_ =>
        {
            isFactoryCalled = true;
            return null!;
        });

        await sut.ValidateAsync(1, errors);

        Assert.False(isFactoryCalled);
    }
}
