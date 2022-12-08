// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Namotion.Reflection;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Squidex.Areas.Api.Config.OpenApi;

public sealed class TagXmlProcessor : IDocumentProcessor
{
    public void Process(DocumentProcessorContext context)
    {
        try
        {
            foreach (var controllerType in context.ControllerTypes)
            {
                var attribute = controllerType.GetCustomAttribute<ApiExplorerSettingsAttribute>();

                if (attribute != null)
                {
                    var tag = context.Document.Tags.FirstOrDefault(x => x.Name == attribute.GroupName);

                    if (tag != null)
                    {
                        var description = controllerType.GetXmlDocsSummary();

                        if (description != null)
                        {
                            tag.Description ??= string.Empty;

                            if (!tag.Description.Contains(description, StringComparison.Ordinal))
                            {
                                tag.Description += "\n\n" + description;
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            XmlDocs.ClearCache();
        }
    }
}
