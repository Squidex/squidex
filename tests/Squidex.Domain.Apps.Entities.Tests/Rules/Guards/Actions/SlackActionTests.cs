// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.Guards.Actions
{
    public class SlackActionTests
    {
        [Fact]
        public async Task Should_add_error_if_webhook_url_is_null()
        {
            var action = new SlackAction { WebhookUrl = null };

            var errors = await RuleActionValidator.ValidateAsync(action);

            Assert.NotEmpty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_webhook_url_is_relative()
        {
            var action = new SlackAction { WebhookUrl = new Uri("/invalid", UriKind.Relative) };

            var errors = await RuleActionValidator.ValidateAsync(action);

            Assert.NotEmpty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_webhook_url_is_absolute()
        {
            var action = new SlackAction { WebhookUrl = new Uri("https://squidex.io", UriKind.Absolute) };

            var errors = await RuleActionValidator.ValidateAsync(action);

            Assert.Empty(errors);
        }
    }
}
