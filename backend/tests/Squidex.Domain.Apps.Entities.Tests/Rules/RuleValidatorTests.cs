// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Flows;
using Squidex.Flows.Internal;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Rules;

public class RuleValidatorTests : GivenContext, IClassFixture<TranslationsFixture>
{
    private readonly IFlowManager<FlowEventContext> flowManager = A.Fake<IFlowManager<FlowEventContext>>();
    private readonly RuleValidator sut;

    private sealed record TestFlowStep : FlowStep
    {
        public override ValueTask<FlowStepResult> ExecuteAsync(FlowExecutionContext executionContext,
            CancellationToken ct)
        {
            return default;
        }
    }

    public RuleValidatorTests()
    {
        sut = new RuleValidator(flowManager, AppProvider);
    }

    [Fact]
    public async Task Should_validate_trigger()
    {
        var trigger = new UsageTrigger { NumDays = 50 };

        var errors = new List<ValidationError>();

        await sut.ValidateTriggerAsync(
            trigger,
            AppId.Id,
            (message, properties) => errors.Add(new ValidationError(message, properties)),
            CancellationToken);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Num days must be between 1 and 30.", "NumDays")
            ]);
    }

    [Fact]
    public async Task Should_validate_step()
    {
        var step = new TestFlowStep();

        A.CallTo(() => flowManager.ValidateAsync(step, A<AddError>._, CancellationToken))
            .Invokes(x =>
            {
                x.GetArgument<AddError>(1)?.Invoke("Path", ValidationErrorType.InvalidProperty, "Invalid property.");
            });

        var errors = new List<ValidationError>();

        await sut.ValidateStepAsync(
            step,
            (message, properties) => errors.Add(new ValidationError(message, properties)),
            CancellationToken);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Invalid property.", "Path")
            ]);
    }

    [Theory]
    [InlineData(ValidationErrorType.NoSteps, "Flow has no step")]
    [InlineData(ValidationErrorType.NoStartStep, "Flow has no start step")]
    [InlineData(ValidationErrorType.InvalidNextStepId, "Invalid next step ID")]
    [InlineData(ValidationErrorType.InvalidStepId, "Invalid step ID")]
    public async Task Should_validate_flow(ValidationErrorType type, string expectedMessage)
    {
        var flow = new FlowDefinition();

        A.CallTo(() => flowManager.ValidateAsync(flow, A<AddError>._, CancellationToken))
            .Invokes(x =>
            {
                x.GetArgument<AddError>(1)?.Invoke("Path", type);
            });

        var errors = new List<ValidationError>();

        await sut.ValidateFlowAsync(flow,
            (message, properties) => errors.Add(new ValidationError(message, properties)),
            CancellationToken);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError(expectedMessage, "Path")
            ]);
    }
}
