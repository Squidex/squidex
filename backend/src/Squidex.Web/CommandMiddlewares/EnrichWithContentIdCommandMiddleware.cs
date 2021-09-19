// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure.Commands;

namespace Squidex.Web.CommandMiddlewares
{
    public sealed class EnrichWithContentIdCommandMiddleware : ICommandMiddleware
    {
        private const string SingletonId = "_schemaId_";

        public Task HandleAsync(CommandContext context, NextDelegate next)
        {
            if (context.Command is ContentCommand contentCommand && contentCommand is not CreateContent)
            {
                if (contentCommand.ContentId.ToString().Equals(SingletonId, StringComparison.Ordinal))
                {
                    contentCommand.ContentId = contentCommand.SchemaId.Id;
                }
            }

            return next(context);
        }
    }
}
