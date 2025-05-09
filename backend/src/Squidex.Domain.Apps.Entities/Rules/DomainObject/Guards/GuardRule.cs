// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject.Guards;

public static class GuardRule
{
    public static Task CanCreate(CreateRule command, IRuleValidator validator,
        CancellationToken ct)
    {
        Guard.NotNull(command);

        return Validate.It(async e =>
        {
            if (command.Trigger == null)
            {
                e(Not.Defined(nameof(command.Trigger)), nameof(command.Trigger));
            }
            else
            {
                await validator.ValidateTriggerAsync(command.Trigger, command.AppId.Id,
                    Validate.WithPrefix(nameof(command.Trigger), e), ct);
            }

            if (command.Flow == null)
            {
                e(Not.Defined(nameof(command.Flow)), nameof(command.Flow));
            }
            else
            {
                await validator.ValidateFlowAsync(command.Flow,
                    Validate.WithPrefix(nameof(command.Flow), e), ct);
            }
        });
    }

    public static Task CanUpdate(UpdateRule command, Rule rule, IRuleValidator validator,
        CancellationToken ct)
    {
        Guard.NotNull(command);

        return Validate.It(async e =>
        {
            if (command.Trigger != null)
            {
                await validator.ValidateTriggerAsync(command.Trigger, command.AppId.Id,
                    Validate.WithPrefix(nameof(command.Trigger), e), ct);
            }

            if (command.Flow != null)
            {
                await validator.ValidateFlowAsync(command.Flow,
                    Validate.WithPrefix(nameof(command.Flow), e), ct);
            }
        });
    }
}
