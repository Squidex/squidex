// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using FluentAssertions;
using Squidex.Domain.Apps.Rules.Action.Webhook;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.Guards.Actions
{
    public class WebhookActionTests
    {
        [Fact]
        public void Should_add_error_if_url_is_null()
        {
            var action = new WebhookAction { Url = null };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Url field is required.", "Url")
                });
        }

        [Fact]
        public void Should_add_error_if_url_is_relative()
        {
            var action = new WebhookAction { Url = new Uri("/invalid", UriKind.Relative) };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Url field must be an absolute URL.", "Url")
                });
        }

        [Fact]
        public void Should_not_add_error_if_url_is_absolute()
        {
            var action = new WebhookAction { Url = new Uri("https://squidex.io", UriKind.Absolute) };

            var errors = action.Validate();

            Assert.Empty(errors);
        }
    }
}
