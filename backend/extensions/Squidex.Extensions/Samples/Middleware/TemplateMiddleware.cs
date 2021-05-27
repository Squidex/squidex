// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Templates.Builders;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Extensions.Samples.Middleware
{
    public sealed class TemplateMiddleware : ICustomCommandMiddleware
    {
        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            await next(context);

            if (context.Command is CreateApp createApp && context.IsCompleted && createApp.Template == "custom")
            {
                var appId = NamedId.Of(createApp.AppId, createApp.Name);

                var publish = new Func<IAppCommand, Task>(command =>
                {
                    command.AppId = appId;

                    return context.CommandBus.PublishAsync(command);
                });

                var schema =
                    SchemaBuilder.Create("Pages")
                        .AddString("Title", f => f
                            .Length(100)
                            .Required())
                        .AddString("Slug", f => f
                            .Length(100)
                            .Required()
                            .Disabled())
                        .AddString("Text", f => f
                            .Length(1000)
                            .Required()
                            .AsRichText())
                        .Build();

                await publish(schema);
            }
        }
    }
}
