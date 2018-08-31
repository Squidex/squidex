// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using FluentAssertions;
using Squidex.Domain.Apps.Rules.Action.Twitter;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.Guards.Actions
{
    public class TweetActionTests
    {
        [Fact]
        public void Should_add_error_if_access_token_is_null()
        {
            var action = new TweetAction { AccessToken = null, AccessSecret = "secret", Text = "text" };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Access Token field is required.", "AccessToken")
                });
        }

        [Fact]
        public void Should_add_error_if_access_secret_is_null()
        {
            var action = new TweetAction { AccessToken = "token", AccessSecret = null, Text = "text" };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Access Secret field is required.", "AccessSecret")
                });
        }

        [Fact]
        public void Should_add_error_if_text_is_null()
        {
            var action = new TweetAction { AccessToken = "token", AccessSecret = "secret", Text = null };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Text field is required.", "Text")
                });
        }

        [Fact]
        public void Should_not_add_error_if_access_token_and_secret_defined()
        {
            var action = new TweetAction { AccessToken = "token", AccessSecret = "secret", Text = "text" };

            var errors = action.Validate();

            Assert.Empty(errors);
        }
    }
}
