// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Apps.Templates
{
    public sealed class CreateBlogCommandMiddleware : ICommandMiddleware
    {
        private const string TemplateName = "Blog";
        private const string SlugScript = @"
            var data = ctx.data;

            data.slug = { iv: slugify(data.title.iv) };

            replace(data);";

        public Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.IsCompleted && context.Command is CreateApp createApp && IsRightTemplate(createApp))
            {
                var appId = new NamedId<Guid>(createApp.AppId, createApp.Name);

                var publish = new Func<ICommand, Task>(command =>
                {
                    if (command is IAppCommand appCommand)
                    {
                        appCommand.AppId = appId;
                    }

                    return context.CommandBus.PublishAsync(command);
                });

                return Task.WhenAll(
                    CreatePagesAsync(publish),
                    CreatePostsAsync(publish),
                    CreateClientAsync(publish, appId.Id));
            }

            return next();
        }

        private static bool IsRightTemplate(CreateApp createApp)
        {
            return string.Equals(createApp.Template, TemplateName, StringComparison.OrdinalIgnoreCase);
        }

        private static async Task CreateClientAsync(Func<ICommand, Task> publish, Guid appId)
        {
            await publish(new AttachClient { Id = "sample-client", AppId = appId });
        }

        private async Task CreatePostsAsync(Func<ICommand, Task> publish)
        {
            var postsId = await CreatePostsSchemaAsync(publish);

            await publish(new CreateContent
            {
                SchemaId = postsId,
                Data =
                    new NamedContentData()
                        .AddField("title",
                            new ContentFieldData()
                                .AddValue("iv", "My first post with Squidex"))
                        .AddField("text",
                            new ContentFieldData()
                                .AddValue("iv", "Just created a blog with Squidex. I love it!")),
                Publish = true,
            });
        }

        private async Task CreatePagesAsync(Func<ICommand, Task> publish)
        {
            var pagesId = await CreatePagesSchemaAsync(publish);

            await publish(new CreateContent
            {
                SchemaId = pagesId,
                Data =
                    new NamedContentData()
                        .AddField("title",
                            new ContentFieldData()
                                .AddValue("iv", "About Me"))
                        .AddField("text",
                            new ContentFieldData()
                                .AddValue("iv", "I love Squidex and SciFi!")),
                Publish = true
            });
        }

        private async Task<NamedId<Guid>> CreatePostsSchemaAsync(Func<ICommand, Task> publish)
        {
            var command = new CreateSchema
            {
                Name = "posts",
                Publish = true,
                Properties = new SchemaProperties
                {
                    Label = "Posts"
                },
                Fields = new List<CreateSchemaField>
                {
                    new CreateSchemaField
                    {
                        Name = "title",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Input,
                            IsRequired = true,
                            IsListField = true,
                            MaxLength = 100,
                            MinLength = 0,
                            Label = "Title"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "slug",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Slug,
                            IsRequired = false,
                            IsListField = true,
                            MaxLength = 100,
                            MinLength = 0,
                            Label = "Slug (Autogenerated)"
                        },
                        IsDisabled = true
                    },
                    new CreateSchemaField
                    {
                        Name = "text",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.RichText,
                            IsRequired = true,
                            IsListField = false,
                            Label = "Text"
                        }
                    }
                }
            };

            await publish(command);

            var schemaId = new NamedId<Guid>(command.SchemaId, command.Name);

            await publish(new ConfigureScripts
            {
                SchemaId = schemaId.Id,
                ScriptCreate = SlugScript,
                ScriptUpdate = SlugScript
            });

            return schemaId;
        }

        private async Task<NamedId<Guid>> CreatePagesSchemaAsync(Func<ICommand, Task> publish)
        {
            var command = new CreateSchema
            {
                Name = "pages",
                Properties = new SchemaProperties
                {
                    Label = "Pages"
                },
                Fields = new List<CreateSchemaField>
                {
                    new CreateSchemaField
                    {
                        Name = "title",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Input,
                            IsRequired = true,
                            IsListField = true,
                            MaxLength = 100,
                            MinLength = 0,
                            Label = "Title"
                        }
                    },
                    new CreateSchemaField
                    {
                        Name = "slug",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.Slug,
                            IsRequired = false,
                            IsListField = true,
                            MaxLength = 100,
                            MinLength = 0,
                            Label = "Slug (Autogenerated)"
                        },
                        IsDisabled = true
                    },
                    new CreateSchemaField
                    {
                        Name = "text",
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.RichText,
                            IsRequired = true,
                            IsListField = false,
                            Label = "Text"
                        }
                    }
                }
            };

            await publish(command);

            var schemaId = new NamedId<Guid>(command.SchemaId, command.Name);

            await publish(new ConfigureScripts
            {
                SchemaId = schemaId.Id,
                ScriptCreate = SlugScript,
                ScriptUpdate = SlugScript
            });

            return schemaId;
        }
    }
}
