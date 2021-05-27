// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps.Templates;
using Squidex.Domain.Apps.Entities.Apps.Templates.Builders;

namespace Squidex.Extensions.Samples.Middleware
{
    public class TemplateInstance : ITemplate
    {
        public string Name { get; } = "custom2";

        public Task RunAsync(PublishTemplate publish)
        {
            var schema =
                SchemaBuilder.Create("Blogs")
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

            return publish(schema);
        }
    }
}
