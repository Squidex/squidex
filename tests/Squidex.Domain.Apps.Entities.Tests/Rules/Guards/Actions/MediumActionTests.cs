// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using FluentAssertions;
using Squidex.Domain.Apps.Rules.Action.Medium;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.Guards.Actions
{
    public class MediumActionTests
    {
        [Fact]
        public void Should_add_error_if_access_token_is_null()
        {
            var action = new MediumAction { AccessToken = null, Title = "title", Content = "content" };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Access Token field is required.", "AccessToken")
                });
        }

        [Fact]
        public void Should_add_error_if_title_null()
        {
            var action = new MediumAction { AccessToken = "token", Title = null, Content = "content" };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Title field is required.", "Title")
                });
        }

        [Fact]
        public void Should_add_error_if_content_is_null()
        {
            var action = new MediumAction { AccessToken = "token", Title = "title", Content = null };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Content field is required.", "Content")
                });
        }

        [Fact]
        public void Should_not_add_error_if_values_are_valid()
        {
            var action = new MediumAction { AccessToken = "token", Title = "title", Content = "content" };

            var errors = action.Validate();

            Assert.Empty(errors);
        }
    }
}
