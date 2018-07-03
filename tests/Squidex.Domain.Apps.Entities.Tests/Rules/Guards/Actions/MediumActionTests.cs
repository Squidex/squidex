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
    public class MediumActionTests
    {
        [Fact]
        public async Task Should_add_error_if_access_token_is_null()
        {
            var action = new MediumAction { AccessToken = null, Title = "title", Content = "content" };

            var errors = await RuleActionValidator.ValidateAsync(action);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Access token is required.", "AccessToken")
                });
        }

        [Fact]
        public async Task Should_add_error_if_title_null()
        {
            var action = new MediumAction { AccessToken = "token", Title = null, Content = "content" };

            var errors = await RuleActionValidator.ValidateAsync(action);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Title is required.", "Title")
                });
        }

        [Fact]
        public async Task Should_add_error_if_content_is_null()
        {
            var action = new MediumAction { AccessToken = "token", Title = "title", Content = null };

            var errors = await RuleActionValidator.ValidateAsync(action);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Content is required.", "Content")
                });
        }

        [Fact]
        public async Task Should_not_add_error_if_values_are_valid()
        {
            var action = new MediumAction { AccessToken = "token", Title = "title", Content = "content" };

            var errors = await RuleActionValidator.ValidateAsync(action);

            Assert.Empty(errors);
        }
    }
}
