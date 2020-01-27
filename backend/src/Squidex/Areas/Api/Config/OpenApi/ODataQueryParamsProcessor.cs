// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Squidex.Areas.Api.Config.OpenApi
{
    public sealed class ODataQueryParamsProcessor : IOperationProcessor
    {
        private readonly string supportedPath;
        private readonly string entity;
        private readonly bool supportSearch;

        public ODataQueryParamsProcessor(string supportedPath, string entity, bool supportSearch)
        {
            this.entity = entity;

            this.supportSearch = supportSearch;
            this.supportedPath = supportedPath;
        }

        public bool Process(OperationProcessorContext context)
        {
            if (context.OperationDescription.Path == supportedPath && context.OperationDescription.Method == "get")
            {
                var operation = context.OperationDescription.Operation;

                operation.AddOData(entity, supportSearch);
            }

            return true;
        }
    }
}
