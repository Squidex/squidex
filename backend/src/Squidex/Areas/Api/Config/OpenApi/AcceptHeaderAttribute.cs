// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;
using NSwag;
using NSwag.Annotations;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Areas.Api.Config.OpenApi;

public sealed class AcceptHeader_Unpublished : AcceptHeaderAttribute
{
    public AcceptHeader_Unpublished()
        : base(ContentHeaders.Unpublished, "Return unpublished content items.", JsonObjectType.Boolean)
    {
    }
}

public sealed class AcceptHeader_Flatten : AcceptHeaderAttribute
{
    public AcceptHeader_Flatten()
        : base(ContentHeaders.Flatten, "Provide the data as flat object.", JsonObjectType.Boolean)
    {
    }
}

public sealed class AcceptHeader_Languages : AcceptHeaderAttribute
{
    public AcceptHeader_Languages()
        : base(ContentHeaders.Languages, "Only resolve these languages (comma-separated).")
    {
    }
}

public sealed class AcceptHeader_NoTotal : AcceptHeaderAttribute
{
    public AcceptHeader_NoTotal()
        : base(ContextHeaders.NoTotal, "Do not return the total amount.", JsonObjectType.Boolean)
    {
    }
}

public sealed class AcceptHeader_NoSlowTotal : AcceptHeaderAttribute
{
    public AcceptHeader_NoSlowTotal()
        : base(ContextHeaders.NoSlowTotal, "Do not return the total amount, if it would be slow.", JsonObjectType.Boolean)
    {
    }
}

public class AcceptHeaderAttribute : OpenApiOperationProcessorAttribute
{
    public AcceptHeaderAttribute(string name, string description, JsonObjectType type = JsonObjectType.String)
        : base(typeof(Processor), name, description, type)
    {
    }

    public sealed class Processor : IOperationProcessor
    {
        private readonly string name;
        private readonly string description;
        private readonly JsonObjectType type;

        public Processor(string name, string description, JsonObjectType type)
        {
            this.name = name;
            this.description = description;
            this.type = type;
        }

        public bool Process(OperationProcessorContext context)
        {
            context.OperationDescription.Operation.Parameters.Add(new OpenApiParameter
            {
                Name = name,
                Kind = OpenApiParameterKind.Header,
                Schema = new JsonSchema
                {
                    Type = type
                },
                Description = description,
            });

            return true;
        }
    }
}
