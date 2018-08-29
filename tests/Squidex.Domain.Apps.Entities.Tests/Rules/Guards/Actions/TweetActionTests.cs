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
    public class TweetActionTests
    {
        [Fact]
        public async Task Should_add_error_if_access_token_is_null()
        {
            var action = new TweetAction { AccessToken = null, AccessSecret = "secret" };

            var errors = await RuleActionValidator.ValidateAsync(action);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Access Token is required.", "AccessToken")
                });
        }

        [Fact]
        public async Task Should_add_error_if_access_secret_is_null()
        {
            var action = new TweetAction { AccessToken = "token", AccessSecret = null };

            var errors = await RuleActionValidator.ValidateAsync(action);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Access Secret is required.", "AccessSecret")
                });
        }

        [Fact]
        public async Task Should_not_add_error_if_access_token_and_secret_defined()
        {
            var action = new TweetAction { AccessToken = "token", AccessSecret = "secret" };

            var errors = await RuleActionValidator.ValidateAsync(action);

            Assert.Empty(errors);
        }
    }
}
