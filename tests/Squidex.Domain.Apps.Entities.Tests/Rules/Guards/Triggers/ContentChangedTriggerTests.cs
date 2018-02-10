// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.Schemas;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.Guards.Triggers
{
    public class ContentChangedTriggerTests
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly Guid appId = Guid.NewGuid();

        [Fact]
        public async Task Should_add_error_if_schemas_ids_are_not_valid()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId, A<Guid>.Ignored, false))
                .Returns(Task.FromResult<ISchemaEntity>(null));

            var trigger = new ContentChangedTrigger
            {
                Schemas = ImmutableList.Create(
                    new ContentChangedTriggerSchema()
                )
            };

            var errors = await RuleTriggerValidator.ValidateAsync(appId, trigger, appProvider);

            Assert.NotEmpty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_schemas_is_null()
        {
            var trigger = new ContentChangedTrigger();

            var errors = await RuleTriggerValidator.ValidateAsync(appId, trigger, appProvider);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_schemas_is_empty()
        {
            var trigger = new ContentChangedTrigger
            {
                Schemas = ImmutableList<ContentChangedTriggerSchema>.Empty
            };

            var errors = await RuleTriggerValidator.ValidateAsync(appId, trigger, appProvider);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_schemas_ids_are_valid()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId, A<Guid>.Ignored, false))
                .Returns(A.Fake<ISchemaEntity>());

            var trigger = new ContentChangedTrigger
            {
                Schemas = ImmutableList.Create(
                    new ContentChangedTriggerSchema()
                )
            };

            var errors = await RuleTriggerValidator.ValidateAsync(appId, trigger, appProvider);

            Assert.Empty(errors);
        }
    }
}
