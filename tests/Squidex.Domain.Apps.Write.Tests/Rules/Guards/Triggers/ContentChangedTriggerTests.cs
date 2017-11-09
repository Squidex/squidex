// ==========================================================================
//  ContentChangedTriggerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Xunit;

namespace Squidex.Domain.Apps.Write.Rules.Guards.Triggers
{
    public class ContentChangedTriggerTests
    {
        private readonly ISchemaProvider schemas = A.Fake<ISchemaProvider>();

        [Fact]
        public async Task Should_add_error_if_schemas_ids_are_not_valid()
        {
            A.CallTo(() => schemas.FindSchemaByIdAsync(A<Guid>.Ignored, false))
                .Returns(Task.FromResult<ISchemaEntity>(null));

            var trigger = new ContentChangedTrigger
            {
                Schemas = new List<ContentChangedTriggerSchema>
                {
                    new ContentChangedTriggerSchema()
                }
            };

            var errors = await RuleTriggerValidator.ValidateAsync(trigger, schemas);

            Assert.NotEmpty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_schemas_is_null()
        {
            var trigger = new ContentChangedTrigger();

            var errors = await RuleTriggerValidator.ValidateAsync(trigger, schemas);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_schemas_is_empty()
        {
            var trigger = new ContentChangedTrigger
            {
                Schemas = new List<ContentChangedTriggerSchema>()
            };

            var errors = await RuleTriggerValidator.ValidateAsync(trigger, schemas);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_schemas_ids_are_valid()
        {
            A.CallTo(() => schemas.FindSchemaByIdAsync(A<Guid>.Ignored, false))
                .Returns(A.Fake<ISchemaEntity>());

            var trigger = new ContentChangedTrigger
            {
                Schemas = new List<ContentChangedTriggerSchema>
                {
                    new ContentChangedTriggerSchema()
                }
            };

            var errors = await RuleTriggerValidator.ValidateAsync(trigger, schemas);

            Assert.Empty(errors);
        }
    }
}
