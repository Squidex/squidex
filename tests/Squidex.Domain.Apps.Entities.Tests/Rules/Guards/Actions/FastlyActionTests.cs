// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using FluentAssertions;
using Squidex.Domain.Apps.Rules.Action.Fastly;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.Guards.Actions
{
    public class FastlyActionTests
    {
        [Fact]
        public void Should_add_error_if_service_id_not_defined()
        {
            var action = new FastlyAction { ServiceId = null, ApiKey = "KEY" };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Service Id field is required.", "ServiceId")
                });
        }

        [Fact]
        public void Should_add_error_if_api_key_not_defined()
        {
            var action = new FastlyAction { ServiceId = "APP", ApiKey = null };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Api Key field is required.", "ApiKey")
                });
        }

        [Fact]
        public void Should_not_add_error_everything_defined()
        {
            var action = new FastlyAction { ServiceId = "APP", ApiKey = "KEY" };

            var errors = action.Validate();

            Assert.Empty(errors);
        }
    }
}
