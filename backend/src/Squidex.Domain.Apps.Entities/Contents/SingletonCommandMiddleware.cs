// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed class SingletonCommandMiddleware : ICommandMiddleware
{
    public async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        await next(context, ct);

        if (context.IsCompleted && context.Command is CreateSchema { Type: SchemaType.Singleton } createSchema)
        {
            var schemaId = NamedId.Of(createSchema.SchemaId, createSchema.Name);

            var data = new ContentData();

            var contentId = schemaId.Id;
            var content = new CreateContent
            {
                Data = data,
                ContentId = contentId,
                DoNotScript = true,
                DoNotValidate = true,
                SchemaId = schemaId,
                Status = Status.Published
            };

            SimpleMapper.Map(createSchema, content);

            // Always create the corresponding content and therefore do not pass over cancellation token.
            await context.CommandBus.PublishAsync(content, default);
        }
    }
}
