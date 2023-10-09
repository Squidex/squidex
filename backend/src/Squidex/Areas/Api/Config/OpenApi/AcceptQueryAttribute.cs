// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NSwag.Annotations;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Squidex.Areas.Api.Config.OpenApi;

public sealed class AcceptQueryAttribute : OpenApiOperationProcessorAttribute
{
    public AcceptQueryAttribute(bool supportsSearch)
        : base(typeof(Processor), supportsSearch)
    {
    }

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
    public sealed record Processor(bool SupportsSearch) : IOperationProcessor
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
    {
        public bool Process(OperationProcessorContext context)
        {
            context.OperationDescription.Operation.AddQuery(SupportsSearch);
            return true;
        }
    }
}
