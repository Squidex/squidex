// ==========================================================================
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
        if (context.Command is IUpsertCommand upsert)
        {
            upsert.FieldsInLists = upsert.FieldsInLists?.Migrate();
            upsert.FieldsInReferences = upsert.FieldsInReferences?.Migrate();
        }

        if (context.Command is ConfigureUIFields configure)
        {
            configure.FieldsInLists = configure.FieldsInLists?.Migrate();
            configure.FieldsInReferences = configure.FieldsInReferences?.Migrate();
        }

        return next(context, ct);
    }
}
