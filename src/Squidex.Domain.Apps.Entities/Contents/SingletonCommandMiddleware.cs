// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class SingletonCommandMiddleware : ICommandMiddleware
    {
        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            await next();

            if (context.IsCompleted &&
                context.Command is CreateSchema createSchema &&
                createSchema.Singleton)
            {
                var schemaId = new NamedId<Guid>(createSchema.SchemaId, createSchema.Name);

                var data = new NamedContentData();

                var contentId = schemaId.Id;
                var content = new CreateContent { Data = data, ContentId = contentId, SchemaId = schemaId, Publish = true };

                SimpleMapper.Map(context.Command, content);

                await context.CommandBus.PublishAsync(content);
            }
        }
    }
}
