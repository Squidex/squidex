// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject.Guards.Triggers;

public class ContentChangedTriggerTests : IClassFixture<TranslationsFixture>
{
    private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");

    [Fact]
    public async Task Should_add_error_if_schema_id_is_not_defined()
    {
        var trigger = new ContentChangedTriggerV2
        {
            Schemas = ReadonlyList.Create(new ContentChangedTriggerSchemaV2())
        };

        var errors = await RuleTriggerValidator.ValidateAsync(appId.Id, trigger, appProvider);

        errors.Should().BeEquivalentTo(
            new List<ValidationError>
            {
                new ValidationError("Schema ID is required.", "Schemas")
            });

        A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, A<DomainId>._, false, default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_add_error_if_schemas_ids_are_not_valid()
    {
        A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Id, false, default))
            .Returns(Task.FromResult<ISchemaEntity?>(null));

        var trigger = new ContentChangedTriggerV2
        {
            Schemas = ReadonlyList.Create(new ContentChangedTriggerSchemaV2 { SchemaId = schemaId.Id })
        };

        var errors = await RuleTriggerValidator.ValidateAsync(appId.Id, trigger, appProvider);

        errors.Should().BeEquivalentTo(
            new List<ValidationError>
            {
                new ValidationError($"Schema {schemaId.Id} does not exist.", "Schemas")
            });
    }

    [Fact]
    public async Task Should_not_add_error_if_schemas_is_null()
    {
        var trigger = new ContentChangedTriggerV2();

        var errors = await RuleTriggerValidator.ValidateAsync(appId.Id, trigger, appProvider);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_schemas_is_empty()
    {
        var trigger = new ContentChangedTriggerV2
        {
            Schemas = ReadonlyList.Empty<ContentChangedTriggerSchemaV2>()
        };

        var errors = await RuleTriggerValidator.ValidateAsync(appId.Id, trigger, appProvider);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_schemas_ids_are_valid()
    {
        A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, A<DomainId>._, false, default))
            .Returns(Mocks.Schema(appId, schemaId));

        var trigger = new ContentChangedTriggerV2
        {
            Schemas = ReadonlyList.Create(new ContentChangedTriggerSchemaV2 { SchemaId = schemaId.Id })
        };

        var errors = await RuleTriggerValidator.ValidateAsync(appId.Id, trigger, appProvider);

        Assert.Empty(errors);
    }
}
