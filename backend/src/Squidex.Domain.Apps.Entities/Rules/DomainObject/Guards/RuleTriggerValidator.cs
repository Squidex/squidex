﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject.Guards;

public sealed class RuleTriggerValidator(DomainId appId, IAppProvider appProvider, CancellationToken ct)
    : IRuleTriggerVisitor<Task<IEnumerable<ValidationError>>>
{
    public static Task<IEnumerable<ValidationError>> ValidateAsync(DomainId appId, RuleTrigger trigger, IAppProvider appProvider,
        CancellationToken ct)
    {
        Guard.NotNull(trigger);
        Guard.NotNull(appProvider);

        var visitor = new RuleTriggerValidator(appId, appProvider, ct);

        return trigger.Accept(visitor);
    }

    public Task<IEnumerable<ValidationError>> Visit(CommentTrigger trigger)
    {
        return Task.FromResult(Enumerable.Empty<ValidationError>());
    }

    public Task<IEnumerable<ValidationError>> Visit(AssetChangedTriggerV2 trigger)
    {
        return Task.FromResult(Enumerable.Empty<ValidationError>());
    }

    public Task<IEnumerable<ValidationError>> Visit(ManualTrigger trigger)
    {
        return Task.FromResult(Enumerable.Empty<ValidationError>());
    }

    public Task<IEnumerable<ValidationError>> Visit(SchemaChangedTrigger trigger)
    {
        return Task.FromResult(Enumerable.Empty<ValidationError>());
    }

    public Task<IEnumerable<ValidationError>> Visit(UsageTrigger trigger)
    {
        var errors = new List<ValidationError>();

        if (trigger.NumDays is < 1 or > 30)
        {
            errors.Add(new ValidationError(Not.Between(nameof(trigger.NumDays), 1, 30), nameof(trigger.NumDays)));
        }

        return Task.FromResult<IEnumerable<ValidationError>>(errors);
    }

    public async Task<IEnumerable<ValidationError>> Visit(ContentChangedTriggerV2 trigger)
    {
        var errors = new List<ValidationError>();

        if (trigger.Schemas == null)
        {
            return errors;
        }

        foreach (var schema in trigger.Schemas)
        {
            if (schema.SchemaId == DomainId.Empty)
            {
                errors.Add(new ValidationError(Not.Defined("SchemaId"), nameof(trigger.Schemas)));
            }
            else if (await appProvider.GetSchemaAsync(appId, schema.SchemaId, false, ct) == null)
            {
                errors.Add(new ValidationError(T.Get("schemas.notFoundId", new { id = schema.SchemaId }), nameof(trigger.Schemas)));
            }
        }

        return errors;
    }
}
