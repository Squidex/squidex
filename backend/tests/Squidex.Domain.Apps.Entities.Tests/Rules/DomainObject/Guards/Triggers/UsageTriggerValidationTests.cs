// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject.Guards.Triggers
{
    public class UsageTriggerValidationTests : IClassFixture<TranslationsFixture>
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly DomainId appId = DomainId.NewGuid();

        [Fact]
        public async Task Should_add_error_if_num_days_less_than_1()
        {
            var trigger = new UsageTrigger { NumDays = 0 };

            var errors = await RuleTriggerValidator.ValidateAsync(appId, trigger, appProvider);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Num days must be between 1 and 30.", "NumDays")
                });
        }

        [Fact]
        public async Task Should_add_error_if_num_days_greater_than_30()
        {
            var trigger = new UsageTrigger { NumDays = 32 };

            var errors = await RuleTriggerValidator.ValidateAsync(appId, trigger, appProvider);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Num days must be between 1 and 30.", "NumDays")
                });
        }

        [Fact]
        public async Task Should_not_add_error_if_num_days_is_valid()
        {
            var trigger = new UsageTrigger { NumDays = 20 };

            var errors = await RuleTriggerValidator.ValidateAsync(appId, trigger, appProvider);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_num_days_is_not_defined()
        {
            var trigger = new UsageTrigger();

            var errors = await RuleTriggerValidator.ValidateAsync(appId, trigger, appProvider);

            Assert.Empty(errors);
        }
    }
}
