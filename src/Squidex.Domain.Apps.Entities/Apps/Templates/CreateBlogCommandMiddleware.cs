// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Tasks;

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

                return Task.WhenAll(
                    CreatePagesAsync(context.CommandBus, appId),
                    CreatePostsAsync(context.CommandBus, appId),
                    CreateClientAsync(context.CommandBus, appId));
            }

            return TaskHelper.Done;
        }

        private static bool IsRightTemplate(CreateApp createApp)
        {
            return string.Equals(createApp.Template, TemplateName, StringComparison.OrdinalIgnoreCase);
        }

        private static async Task CreateClientAsync(ICommandBus bus, NamedId<Guid> appId)
        {
            await bus.PublishAsync(new AttachClient { Id = "sample-client" });
        }

        private async Task CreatePostsAsync(ICommandBus bus, NamedId<Guid> appId)
        {
            var postsId = await CreatePostsSchema(bus, appId);

            await bus.PublishAsync(new CreateContent
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

        private async Task CreatePagesAsync(ICommandBus bus, NamedId<Guid> appId)
        {
            var pagesId = await CreatePagesSchema(bus, appId);

            await bus.PublishAsync(new CreateContent
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

        private async Task<NamedId<Guid>> CreatePostsSchema(ICommandBus bus, NamedId<Guid> appId)
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
                        Partitioning = Partitioning.Invariant.Key,
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
                        Partitioning = Partitioning.Invariant.Key,
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
                        Partitioning = Partitioning.Invariant.Key,
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.RichText,
                            IsRequired = true,
                            IsListField = false,
                            Label = "Text"
                        }
                    }
                },
                AppId = appId
            };

            await bus.PublishAsync(command);

            var schemaId = new NamedId<Guid>(command.SchemaId, command.Name);

            await bus.PublishAsync(new ConfigureScripts
            {
                SchemaId = schemaId.Id,
                ScriptCreate = SlugScript,
                ScriptUpdate = SlugScript
            });

            return schemaId;
        }

        private async Task<NamedId<Guid>> CreatePagesSchema(ICommandBus bus, NamedId<Guid> appId)
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
                        Partitioning = Partitioning.Invariant.Key,
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
                        Partitioning = Partitioning.Invariant.Key,
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
                        Partitioning = Partitioning.Invariant.Key,
                        Properties = new StringFieldProperties
                        {
                            Editor = StringFieldEditor.RichText,
                            IsRequired = true,
                            IsListField = false,
                            Label = "Text"
                        }
                    }
                },
                AppId = appId
            };

            await bus.PublishAsync(command);

            var schemaId = new NamedId<Guid>(command.SchemaId, command.Name);

            await bus.PublishAsync(new ConfigureScripts
            {
                SchemaId = schemaId.Id,
                ScriptCreate = SlugScript,
                ScriptUpdate = SlugScript
            });

            return schemaId;
        }
    }
}
