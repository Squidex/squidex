// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentImporterCommandMiddleware : ICommandMiddleware
    {
        private readonly IServiceProvider serviceProvider;

        public ContentImporterCommandMiddleware(IServiceProvider serviceProvider)
        {
            Guard.NotNull(serviceProvider);

            this.serviceProvider = serviceProvider;
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            if (context.Command is CreateContents createContents)
            {
                var result = new ImportResult();

                if (createContents.Datas != null && createContents.Datas.Count > 0)
                {
                    var command = SimpleMapper.Map(createContents, new CreateContent());

                    foreach (var data in createContents.Datas)
                    {
                        try
                        {
                            command.ContentId = Guid.NewGuid();
                            command.Data = data;

                            var content = serviceProvider.GetRequiredService<ContentDomainObject>();

                            content.Setup(command.ContentId);

                            await content.ExecuteAsync(command);

                            result.Add(new ImportResultItem { ContentId = command.ContentId });
                        }
                        catch (Exception ex)
                        {
                            result.Add(new ImportResultItem { Exception = ex });
                        }
                    }
                }

                context.Complete(result);
            }
            else
            {
                await next(context);
            }
        }
    }
}
