// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Apps.Templates
{
    public delegate Task PublishTemplate(IAppCommand command);

    public sealed class TemplateCommandMiddleware : ICommandMiddleware
    {
        private readonly Dictionary<string, ITemplate> templates;

        public TemplateCommandMiddleware(IEnumerable<ITemplate> templates)
        {
            this.templates = templates.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            if (context.IsCompleted && context.Command is CreateApp createApp && !string.IsNullOrWhiteSpace(createApp.Template))
            {
                if (templates.TryGetValue(createApp.Template, out var template))
                {
                    var appId = NamedId.Of(createApp.AppId, createApp.Name);

                    var publish = new PublishTemplate(command =>
                    {
                        command.AppId = appId;

                        return context.CommandBus.PublishAsync(command);
                    });

                    await template.RunAsync(publish);
                }
            }

            await next(context);
        }
    }
}
