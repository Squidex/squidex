// ==========================================================================
//  XmlTagProcessor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Reflection;
using NJsonSchema.Infrastructure;
using NSwag.Annotations;
using NSwag.CodeGeneration.SwaggerGenerators.WebApi.Processors;
using NSwag.CodeGeneration.SwaggerGenerators.WebApi.Processors.Contexts;

// ReSharper disable InvertIf

namespace Squidex.Configurations.Swagger
{
    public sealed class XmlTagProcessor : IOperationProcessor, IDocumentProcessor
    {
        public void Process(DocumentProcessorContext context)
        {
            foreach (var controllerType in context.ControllerTypes)
            {
                var tagAttribute =
                    controllerType.GetTypeInfo().GetCustomAttribute<SwaggerTagAttribute>();

                if (tagAttribute != null)
                {
                    var tag = context.Document.Tags.Find(x => x.Name == tagAttribute.Name);

                    if (tag != null)
                    {
                        var description = controllerType.GetXmlSummary();

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
        }

        public bool Process(OperationProcessorContext context)
        {
            var tagAttribute = 
                context.MethodInfo.DeclaringType.GetTypeInfo().GetCustomAttribute<SwaggerTagAttribute>();

            if (tagAttribute != null)
            {
                context.OperationDescription.Operation.Tags.Clear();
                context.OperationDescription.Operation.Tags.Add(tagAttribute.Name);
            }

            return true;
        }
    }
}
