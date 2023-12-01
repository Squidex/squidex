﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Schemas;

public sealed class MigrateFieldNamesCommandMiddleware : ICommandMiddleware
{
    public Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        if (context.Command is SynchronizeSchema synchronize)
        {
            synchronize.FieldsInLists = synchronize.FieldsInLists?.Migrate();
            synchronize.FieldsInReferences = synchronize.FieldsInReferences?.Migrate();
        }

        if (context.Command is ConfigureUIFields configure)
        {
            configure.FieldsInLists = configure.FieldsInLists?.Migrate();
            configure.FieldsInReferences = configure.FieldsInReferences?.Migrate();
        }

        return next(context, ct);
    }
}
