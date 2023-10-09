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
using Squidex.Domain.Apps.Core;

namespace Squidex.Areas.Api.Config.OpenApi;

public sealed class AcceptAnyBodyAttribute : OpenApiOperationProcessorAttribute
{
    public AcceptAnyBodyAttribute()
        : base(typeof(Processor))
    {
    }

    public sealed class Processor : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
        {
            context.OperationDescription.Operation.Parameters.Add(
                new OpenApiParameter
                {
                    Name = "request",
                    Kind = OpenApiParameterKind.Body,
                    Schema = new JsonSchema
                    {
                    },
                    Description = FieldDescriptions.GraphqlRequest
                });

            return true;
        }
    }
}
