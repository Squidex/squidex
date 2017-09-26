// ==========================================================================
//  XmlTagProcessor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Reflection;
using System.Threading.Tasks;
using NJsonSchema.Infrastructure;
using NSwag.Annotations;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Config.Swagger
{
    public sealed class XmlTagProcessor : IOperationProcessor, IDocumentProcessor
    {
        public Task ProcessAsync(DocumentProcessorContext context)
        {
            foreach (var controllerType in context.ControllerTypes)
            {
                var tagAttribute = controllerType.GetTypeInfo().GetCustomAttribute<SwaggerTagAttribute>();

                if (tagAttribute != null)
                {
                    var tag = context.Document.Tags.Find(x => x.Name == tagAttribute.Name);

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

        public Task<bool> ProcessAsync(OperationProcessorContext context)
        {
            var tagAttribute = context.MethodInfo.DeclaringType.GetTypeInfo().GetCustomAttribute<SwaggerTagAttribute>();

            if (tagAttribute != null)
            {
                context.OperationDescription.Operation.Tags.Clear();
                context.OperationDescription.Operation.Tags.Add(tagAttribute.Name);
            }

            return TaskHelper.True;
        }
    }
}
