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
        public async Task Should_add_error_if_pin_code_is_null()
        {
            var action = new TweetAction { PinCode = null };

            var errors = await RuleActionValidator.ValidateAsync(action);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Pin Code is required.", "PinCode")
                });
        }

        [Fact]
        public async Task Should_not_add_error_if_pin_code_is_absolute()
        {
            var action = new TweetAction { PinCode = "123" };

            var errors = await RuleActionValidator.ValidateAsync(action);

            Assert.Empty(errors);
        }
    }
}
