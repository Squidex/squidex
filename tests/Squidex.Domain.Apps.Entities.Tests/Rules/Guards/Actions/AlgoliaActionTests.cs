// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using FluentAssertions;
using Squidex.Domain.Apps.Rules.Action.Algolia;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.Guards.Actions
{
    public class AlgoliaActionTests
    {
        [Fact]
        public void Should_add_error_if_app_id_not_defined()
        {
            var action = new AlgoliaAction { AppId = null, ApiKey = "KEY", IndexName = "IDX" };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Application Id field is required.", "AppId")
                });
        }

        [Fact]
        public void Should_add_error_if_api_key_not_defined()
        {
            var action = new AlgoliaAction { AppId = "APP", ApiKey = null, IndexName = "IDX" };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Api Key field is required.", "ApiKey")
                });
        }

        [Fact]
        public void Should_add_error_if_index_name_not_defined()
        {
            var action = new AlgoliaAction { AppId = "APP", ApiKey = "KEY", IndexName = null };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Index Name field is required.", "IndexName")
                });
        }

        [Fact]
        public void Should_not_add_error_everything_defined()
        {
            var action = new AlgoliaAction { AppId = "APP", ApiKey = "KEY", IndexName = "IDX" };

            var errors = action.Validate();

            Assert.Empty(errors);
        }
    }
}
