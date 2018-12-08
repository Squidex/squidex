// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NJsonSchema.Infrastructure;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Areas.Api.Config.Swagger
{
    public sealed class XmlTagProcessor : IDocumentProcessor
    {
        public Task ProcessAsync(DocumentProcessorContext context)
        {
            foreach (var controllerType in context.ControllerTypes)
            {
                var attribute = controllerType.GetTypeInfo().GetCustomAttribute<ApiExplorerSettingsAttribute>();

                if (attribute != null)
                {
                    var tag = context.Document.Tags.FirstOrDefault(x => x.Name == attribute.GroupName);

                    if (tag != null)
                    {
                        var description = controllerType.GetXmlSummaryAsync().Result;

                        if (description != null)
                        {
                            tag.Description = tag.Description ?? string.Empty;

                            if (!tag.Description.Contains(description))
                            {
                                tag.Description += "\n\n" + description;
                            }
                        }
                    }
                }
            }

            return TaskHelper.Done;
        }
    }
}
