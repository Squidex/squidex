// ==========================================================================
//  WebhookActionTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Xunit;

namespace Squidex.Domain.Apps.Write.Rules.Guards.Actions
{
    public sealed class WebhookActionTests
    {
        [Fact]
        public async Task Should_add_error_if_url_is_null()
        {
            var action = new WebhookAction { Url = null };

            var errors = await RuleActionValidator.ValidateAsync(action);

            Assert.NotEmpty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_url_is_relative()
        {
            var action = new WebhookAction { Url = new Uri("/invalid", UriKind.Relative) };

            var errors = await RuleActionValidator.ValidateAsync(action);

            Assert.NotEmpty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_url_is_absolute()
        {
            var action = new WebhookAction { Url = new Uri("https://squidex.io", UriKind.Absolute) };

            var errors = await RuleActionValidator.ValidateAsync(action);

            Assert.Empty(errors);
        }
    }
}
