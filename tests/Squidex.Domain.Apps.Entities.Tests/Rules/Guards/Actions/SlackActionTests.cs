// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using FluentAssertions;
using Squidex.Domain.Apps.Rules.Action.Slack;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.Guards.Actions
{
    public class SlackActionTests
    {
        [Fact]
        public void Should_add_error_if_webhook_url_is_null()
        {
            var action = new SlackAction { WebhookUrl = null, Text = "text" };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Webhook Url field is required.", "WebhookUrl")
                });
        }

        [Fact]
        public void Should_add_error_if_webhook_url_is_relative()
        {
            var action = new SlackAction { WebhookUrl = new Uri("/invalid", UriKind.Relative), Text = "text" };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Webhook Url field must be an absolute URL.", "WebhookUrl")
                });
        }

        [Fact]
        public void Should_add_error_if_text_is_null()
        {
            var action = new SlackAction { WebhookUrl = new Uri("https://squidex.io", UriKind.Absolute), Text = null };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Text field is required.", "Text")
                });
        }

        [Fact]
        public void Should_not_add_error_if_webhook_url_is_absolute()
        {
            var action = new SlackAction { WebhookUrl = new Uri("https://squidex.io", UriKind.Absolute), Text = "text" };

            var errors = action.Validate();

            Assert.Empty(errors);
        }
    }
}
