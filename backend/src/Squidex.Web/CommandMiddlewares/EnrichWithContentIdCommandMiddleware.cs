// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure.Commands;

namespace Squidex.Web.CommandMiddlewares;

public sealed class EnrichWithContentIdCommandMiddleware : ICommandMiddleware
{
    private const string SingletonId = "_schemaId_";

    public Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        if (context.Command is ContentCommand contentCommand and not CreateContent)
        {
            if (contentCommand.ContentId.ToString().Equals(SingletonId, StringComparison.Ordinal))
            {
                contentCommand.ContentId = contentCommand.SchemaId.Id;
            }
        }

        return next(context, ct);
    }
}
