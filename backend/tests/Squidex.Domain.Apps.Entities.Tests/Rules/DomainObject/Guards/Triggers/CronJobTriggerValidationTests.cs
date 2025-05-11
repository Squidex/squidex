// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Flows.CronJobs;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject.Guards.Triggers;

public class CronJobTriggerValidationTests : GivenContext, IClassFixture<TranslationsFixture>
{
    private readonly IFlowCronJobManager<CronJobContext> flowCronJobs = A.Fake<IFlowCronJobManager<CronJobContext>>();
    private readonly RuleValidator validator;

    public CronJobTriggerValidationTests()
    {
        A.CallTo(() => flowCronJobs.IsValidCronExpression(A<string>._))
            .Returns(true);

        A.CallTo(() => flowCronJobs.IsValidTimezone(A<string>._))
            .Returns(true);

        validator = new RuleValidator(null!, flowCronJobs, AppProvider);
    }

    [Fact]
    public async Task Should_add_error_if_expression_is_not_valid()
    {
        var trigger = new CronJobTrigger
        {
            CronExpression = "invalid"
        };

        A.CallTo(() => flowCronJobs.IsValidCronExpression("invalid"))
            .Returns(false);

        var errors = await ValidateAsync(trigger);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Cron Expression is invalid.", "CronExpression"),
            ]);
    }

    [Fact]
    public async Task Should_add_error_if_timezone_is_not_valid()
    {
        var trigger = new CronJobTrigger
        {
            CronTimezone = "invalid"
        };

        A.CallTo(() => flowCronJobs.IsValidTimezone("invalid"))
            .Returns(false);

        var errors = await ValidateAsync(trigger);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Timezone is not a valid IANA identifier.", "CronTimezone"),
            ]);
    }

    [Fact]
    public async Task Should_not_add_error_if_valid()
    {
        var trigger = new CronJobTrigger();

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
