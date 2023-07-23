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

public class AcceptHeader
{
    public sealed class UnpublishedAttribute : BaseAttribute
    {
        public UnpublishedAttribute()
            : base(ContentHeaders.KeyUnpublished, "Return unpublished content items.", JsonObjectType.Boolean)
        {
        }
    }

    public sealed class FieldsAttribute : BaseAttribute
    {
        public FieldsAttribute()
            : base(ContentHeaders.KeyFields, "The list of content fields (comma-separated).", JsonObjectType.String)
        {
        }
    }

    public sealed class FlattenAttribute : BaseAttribute
    {
        public FlattenAttribute()
            : base(ContentHeaders.KeyFlatten, "Provide the data as flat object.", JsonObjectType.Boolean)
        {
        }
    }

    public sealed class LanguagesAttribute : BaseAttribute
    {
        public LanguagesAttribute()
            : base(ContentHeaders.KeyLanguages, "The list of languages to resolve (comma-separated).")
        {
        }
    }

    public sealed class NoTotalAttribute : BaseAttribute
    {
        public NoTotalAttribute()
            : base(ContextHeaders.KeyNoTotal, "Do not return the total amount.", JsonObjectType.Boolean)
        {
        }
    }

    public sealed class NoSlowTotalAttribute : BaseAttribute
    {
        public NoSlowTotalAttribute()
            : base(ContextHeaders.KeyNoSlowTotal, "Do not return the total amount, if it would be slow.", JsonObjectType.Boolean)
        {
        }
    }

    public abstract class BaseAttribute : OpenApiOperationProcessorAttribute
    {
        protected BaseAttribute(string name, string description, JsonObjectType schemaType = JsonObjectType.String)
            : base(typeof(Processor), name, description, schemaType)
        {
        }

#pragma warning disable IDE1006 // Naming Styles
        public record Processor(string name, string description, JsonObjectType schemaType) : IOperationProcessor
#pragma warning restore IDE1006 // Naming Styles
        {
            public bool Process(OperationProcessorContext context)
            {
                var parameter = new OpenApiParameter
                {
                    Name = name,
                    Kind = OpenApiParameterKind.Header,
                    Schema = new JsonSchema
                    {
                        Type = schemaType
                    },
                    Description = description
                };

                context.OperationDescription.Operation.Parameters.Add(parameter);
                context.OperationDescription.Operation.SetPositions();

                return true;
            }
        }
    }
}
