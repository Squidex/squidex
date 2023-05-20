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

    public sealed class Processor : IOperationProcessor
    {
        private readonly bool supportsSearch;

        public Processor(bool supportsSearch)
        {
            this.supportsSearch = supportsSearch;
        }

        public bool Process(OperationProcessorContext context)
        {
            context.OperationDescription.Operation.AddQuery(supportsSearch);
            return true;
        }
    }
}
