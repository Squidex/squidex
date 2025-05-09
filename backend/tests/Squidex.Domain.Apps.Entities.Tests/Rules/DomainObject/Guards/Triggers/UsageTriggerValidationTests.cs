// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject.Guards.Triggers;

public class UsageTriggerValidationTests : GivenContext, IClassFixture<TranslationsFixture>
{
    private readonly IRuleValidator validator;

    public UsageTriggerValidationTests()
    {
        validator = new RuleValidator(null!, AppProvider);
    }

    [Fact]
    public async Task Should_add_error_if_num_days_less_than_1()
    {
        var trigger = new UsageTrigger { NumDays = 0 };

        var errors = await ValidateAsync(trigger);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Num days must be between 1 and 30.", "NumDays"),
            ]);
    }

    [Fact]
    public async Task Should_add_error_if_num_days_greater_than_30()
    {
        var trigger = new UsageTrigger { NumDays = 32 };

        var errors = await ValidateAsync(trigger);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Num days must be between 1 and 30.", "NumDays"),
            ]);
    }

    [Fact]
    public async Task Should_not_add_error_if_num_days_is_valid()
    {
        var trigger = new UsageTrigger { NumDays = 20 };

        var errors = await ValidateAsync(trigger);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_num_days_is_not_defined()
    {
        var trigger = new UsageTrigger();

        var errors = await ValidateAsync(trigger);

        Assert.Empty(errors);
    }

    private async Task<List<ValidationError>> ValidateAsync(RuleTrigger trigger)
    {
        var errors = new List<ValidationError>();

        await validator.ValidateTriggerAsync(trigger, AppId.Id, (m, p) => errors.Add(new ValidationError(m, p)), CancellationToken);
        return errors;
    }
}
