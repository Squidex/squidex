// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Flows;
using Squidex.Flows.Internal;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Rules;

public interface IRuleValidator
{
    Task ValidateTriggerAsync(RuleTrigger trigger, DomainId appId, AddValidation addError,
        CancellationToken ct = default);

    Task ValidateFlowAsync(FlowDefinition flow, AddValidation addError,
        CancellationToken ct = default);

    Task ValidateStepAsync(FlowStep step, AddValidation addError,
        CancellationToken ct = default);
}
