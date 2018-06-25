// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.Guards.Actions
{
    public class AlgoliaActionTests
    {
        [Fact]
        public async Task Should_add_error_if_app_id_not_defined()
        {
            var action = new AlgoliaAction { AppId = null, ApiKey = "KEY", IndexName = "IDX" };

            var errors = await RuleActionValidator.ValidateAsync(action);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Application ID is required.", "AppId")
                });
        }

        [Fact]
        public async Task Should_add_error_if_api_key_not_defined()
        {
            var action = new AlgoliaAction { AppId = "APP", ApiKey = null, IndexName = "IDX" };

            var errors = await RuleActionValidator.ValidateAsync(action);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Api Key is required.", "ApiKey")
                });
        }

        [Fact]
        public async Task Should_add_error_if_index_name_not_defined()
        {
            var action = new AlgoliaAction { AppId = "APP", ApiKey = "KEY", IndexName = null };

            var errors = await RuleActionValidator.ValidateAsync(action);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Index name is required.", "IndexName")
                });
        }

        [Fact]
        public async Task Should_not_add_error_everything_defined()
        {
            var action = new AlgoliaAction { AppId = "APP", ApiKey = "KEY", IndexName = "IDX" };

            var errors = await RuleActionValidator.ValidateAsync(action);

            Assert.Empty(errors);
        }
    }
}
