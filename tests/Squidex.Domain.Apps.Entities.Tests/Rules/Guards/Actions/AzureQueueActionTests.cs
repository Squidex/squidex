// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using FluentAssertions;
using Squidex.Domain.Apps.Rules.Action.AzureQueue;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.Guards.Actions
{
    public class AzureQueueActionTests
    {
        [Fact]
        public void Should_add_error_if_connection_string_is_null()
        {
            var action = new AzureQueueAction { ConnectionString = null, Queue = "squidex" };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Connection String field is required.", "ConnectionString")
                });
        }

        [Fact]
        public void Should_add_error_if_queue_is_null()
        {
            var action = new AzureQueueAction { ConnectionString = "connection", Queue = null };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("The Queue field is required.", "Queue")
                });
        }

        [Fact]
        public void Should_add_error_if_queue_is_invalid()
        {
            var action = new AzureQueueAction { ConnectionString = "connection", Queue = "Squidex" };

            var errors = action.Validate();

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Queue must be valid azure queue name.", "Queue")
                });
        }

        [Fact]
        public void Should_not_add_error_if_values_are_valid()
        {
            var action = new AzureQueueAction { ConnectionString = "connection", Queue = "squidex" };

            var errors = action.Validate();

            Assert.Empty(errors);
        }
    }
}
