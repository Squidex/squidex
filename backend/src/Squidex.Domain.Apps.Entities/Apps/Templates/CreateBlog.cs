﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps.Templates.Builders;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Templates
{
    public sealed class CreateBlog : ITemplate
    {
        public string Name { get; } = "blog";

        public Task RunAsync(PublishTemplate publish)
        {
            return Task.WhenAll(
                CreatePagesAsync(publish),
                CreatePostsAsync(publish));
        }

        private static async Task CreatePostsAsync(PublishTemplate publish)
        {
            var postsId = await CreatePostsSchemaAsync(publish);

            await publish(new CreateContent
            {
                SchemaId = postsId,
                Data =
                    new ContentData()
                        .AddField("title",
                            new ContentFieldData()
                                .AddInvariant("My first post with Squidex"))
                        .AddField("text",
                            new ContentFieldData()
                                .AddInvariant("Just created a blog with Squidex. I love it!")),
                Status = Status.Published
            });
        }

        private static async Task CreatePagesAsync(PublishTemplate publish)
        {
            var pagesId = await CreatePagesSchemaAsync(publish);

            await publish(new CreateContent
            {
                SchemaId = pagesId,
                Data =
                    new ContentData()
                        .AddField("title",
                            new ContentFieldData()
                                .AddInvariant("About Me"))
                        .AddField("text",
                            new ContentFieldData()
                                .AddInvariant("I love Squidex and SciFi!")),
                Status = Status.Published
            });
        }

        private static async Task<NamedId<DomainId>> CreatePostsSchemaAsync(PublishTemplate publish)
        {
            var schema =
                SchemaBuilder.Create("Posts")
                    .AddString("Title", f => f
                        .Properties(p => p with
                        {
                            MaxLength = 100
                        })
                        .Required()
                        .ShowInList()
                        .Hints("The title of the post."))
                    .AddString("Text", f => f
                        .Properties(p => p with
                        {
                            Editor = StringFieldEditor.RichText
                        })
                        .Required()
                        .Hints("The text of the post."))
                    .AddString("Slug", f => f
                        .Disabled()
                        .Label("Slug (Autogenerated)")
                        .Hints("Autogenerated slug that can be used to identity the post."))
                    .WithScripts(DefaultScripts.GenerateSlug)
                    .Build();

            await publish(schema);

            return NamedId.Of(schema.SchemaId, schema.Name);
        }

        private static async Task<NamedId<DomainId>> CreatePagesSchemaAsync(PublishTemplate publish)
        {
            var schema =
                SchemaBuilder.Create("Pages")
                    .AddString("Title", f => f
                        .Properties(p => p with
                        {
                            MaxLength = 100
                        })
                        .Required()
                        .ShowInList()
                        .Hints("The title of the page."))
                    .AddString("Text", f => f
                        .Properties(p => p with
                        {
                            Editor = StringFieldEditor.RichText
                        })
                        .Required()
                        .Hints("The text of the page."))
                    .AddString("Slug", f => f
                        .Disabled()
                        .Label("Slug (Autogenerated)")
                        .Hints("Autogenerated slug that can be used to identity the page."))
                    .WithScripts(DefaultScripts.GenerateSlug)
                    .Build();

            await publish(schema);

            return NamedId.Of(schema.SchemaId, schema.Name);
        }
    }
}
