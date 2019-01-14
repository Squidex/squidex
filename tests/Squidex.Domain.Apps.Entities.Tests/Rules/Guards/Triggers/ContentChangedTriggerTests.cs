// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.Guards.Triggers
{
    public class ContentChangedTriggerTests
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly Guid appId = Guid.NewGuid();
        private readonly Guid schemaId = Guid.NewGuid();

        [Fact]
        public async Task Should_add_error_if_schema_id_is_not_defined()
        {
            var trigger = new ContentChangedTriggerV2
            {
                Schemas = ReadOnlyCollection.Create(new ContentChangedTriggerSchemaV2())
            };

            var errors = await RuleTriggerValidator.ValidateAsync(appId, trigger, appProvider);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Schema id is required.", "Schemas")
                });

            A.CallTo(() => appProvider.GetSchemaAsync(appId, A<Guid>.Ignored, false))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_add_error_if_schemas_ids_are_not_valid()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId, schemaId, false))
                .Returns(Task.FromResult<ISchemaEntity>(null));

            var trigger = new ContentChangedTriggerV2
            {
                Schemas = ReadOnlyCollection.Create(new ContentChangedTriggerSchemaV2 { SchemaId = schemaId })
            };

            var errors = await RuleTriggerValidator.ValidateAsync(appId, trigger, appProvider);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError($"Schema {schemaId} does not exist.", "Schemas")
                });
        }

        [Fact]
        public async Task Should_not_add_error_if_schemas_is_null()
        {
            var trigger = new ContentChangedTriggerV2();

            var errors = await RuleTriggerValidator.ValidateAsync(appId, trigger, appProvider);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_schemas_is_empty()
        {
            var trigger = new ContentChangedTriggerV2
            {
                Schemas = ReadOnlyCollection.Empty<ContentChangedTriggerSchemaV2>()
            };

            var errors = await RuleTriggerValidator.ValidateAsync(appId, trigger, appProvider);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_schemas_ids_are_valid()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId, A<Guid>.Ignored, false))
                .Returns(A.Fake<ISchemaEntity>());

            var trigger = new ContentChangedTriggerV2
            {
                Schemas = ReadOnlyCollection.Create(new ContentChangedTriggerSchemaV2 { SchemaId = schemaId })
            };

            var errors = await RuleTriggerValidator.ValidateAsync(appId, trigger, appProvider);

            Assert.Empty(errors);
        }
    }
}
