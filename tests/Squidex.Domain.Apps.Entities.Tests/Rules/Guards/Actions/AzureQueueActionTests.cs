// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.Guards.Actions
{
    public class AzureQueueActionTests
    {
        [Fact]
        public async Task Should_add_error_if_connection_string_is_null()
        {
            var action = new AzureQueueAction { ConnectionString = null, Queue = "squidex" };

            var errors = await RuleActionValidator.ValidateAsync(action);

            Assert.NotEmpty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_queue_is_null()
        {
            var action = new AzureQueueAction { ConnectionString = "connection", Queue = null };

            var errors = await RuleActionValidator.ValidateAsync(action);

            Assert.NotEmpty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_queue_is_invalid()
        {
            var action = new AzureQueueAction { ConnectionString = "connection", Queue = "Squidex" };

            var errors = await RuleActionValidator.ValidateAsync(action);

            Assert.NotEmpty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_values_are_valid()
        {
            var action = new AzureQueueAction { ConnectionString = "connection", Queue = "squidex" };

            var errors = await RuleActionValidator.ValidateAsync(action);

            Assert.Empty(errors);
        }
    }
}
