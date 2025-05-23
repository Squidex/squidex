﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject.Guards.Triggers;

public class ContentChangedTriggerTests : GivenContext, IClassFixture<TranslationsFixture>
{
    private readonly RuleValidator validator;

    public ContentChangedTriggerTests()
    {
        validator = new RuleValidator(null!, null!, AppProvider);
    }

    [Fact]
    public async Task Should_add_error_if_schema_id_is_not_defined()
    {
        var trigger = new ContentChangedTriggerV2
        {
            Schemas = ReadonlyList.Create(new SchemaCondition()),
        };

        var errors = await ValidateAsync(trigger);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Schema ID is required.", "Schemas"),
            ]);

        A.CallTo(() => AppProvider.GetSchemaAsync(AppId.Id, A<DomainId>._, false, default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_add_error_if_schemas_ids_are_not_valid()
    {
        A.CallTo(() => AppProvider.GetSchemaAsync(AppId.Id, SchemaId.Id, false, CancellationToken))
            .Returns(Task.FromResult<Schema?>(null));

        var trigger = new ContentChangedTriggerV2
        {
            Schemas = ReadonlyList.Create(new SchemaCondition { SchemaId = SchemaId.Id }),
        };

        var errors = await ValidateAsync(trigger);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError($"Schema {SchemaId.Id} does not exist.", "Schemas"),
            ]);
    }

    [Fact]
    public async Task Should_not_add_error_if_schemas_is_null()
    {
        var trigger = new ContentChangedTriggerV2();

        var errors = await ValidateAsync(trigger);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_schemas_is_empty()
    {
        var trigger = new ContentChangedTriggerV2
        {
            Schemas = [],
        };

        var errors = await ValidateAsync(trigger);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_schemas_ids_are_valid()
    {
        A.CallTo(() => AppProvider.GetSchemaAsync(AppId.Id, A<DomainId>._, false, default))
            .Returns(Schema);

        var trigger = new ContentChangedTriggerV2
        {
            Schemas = ReadonlyList.Create(new SchemaCondition { SchemaId = SchemaId.Id }),
        };

        var errors = await ValidateAsync(trigger);

        Assert.Empty(errors);
    }

    private async Task<List<ValidationError>> ValidateAsync(RuleTrigger trigger)
    {
        var errors = new List<ValidationError>();

        await validator.ValidateTriggerAsync(trigger, AppId.Id, (m, p) => errors.Add(new ValidationError(m, p)), CancellationToken);
        return errors;
    }
}
