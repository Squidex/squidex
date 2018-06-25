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
    public class FastlyActionTests
    {
        [Fact]
        public async Task Should_add_error_if_service_id_not_defined()
        {
            var action = new FastlyAction { ServiceId = null, ApiKey = "KEY" };

            var errors = await RuleActionValidator.ValidateAsync(action);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Service ID is required.", "ServiceId")
                });
        }

        [Fact]
        public async Task Should_add_error_if_api_key_not_defined()
        {
            var action = new FastlyAction { ServiceId = "APP", ApiKey = null };

            var errors = await RuleActionValidator.ValidateAsync(action);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Api Key is required.", "ApiKey")
                });
        }

        [Fact]
        public async Task Should_not_add_error_everything_defined()
        {
            var action = new FastlyAction { ServiceId = "APP", ApiKey = "KEY" };

            var errors = await RuleActionValidator.ValidateAsync(action);

            Assert.Empty(errors);
        }
    }
}
