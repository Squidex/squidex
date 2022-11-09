// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Squidex.Areas.Api.Config.OpenApi;

public sealed class QueryParamsProcessor : IOperationProcessor
{
    private readonly string path;

    public QueryParamsProcessor(string path)
    {
        this.path = path;
    }

    public bool Process(OperationProcessorContext context)
    {
        if (context.OperationDescription.Path == path && context.OperationDescription.Method == OpenApiOperationMethod.Get)
        {
            var operation = context.OperationDescription.Operation;

            operation.AddQuery(false);
        }

        return true;
    }
}
