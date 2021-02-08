// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using GraphQL;
using Squidex.Domain.Apps.Entities.Contents.GraphQL;

namespace Squidex.Web.GraphQL
{
    public sealed class DynamicExecutor : IDocumentExecuter
    {
        private readonly IGraphQLService graphQLService;

        public DynamicExecutor(IGraphQLService graphQLService)
        {
            this.graphQLService = graphQLService;
        }

        public Task<ExecutionResult> ExecuteAsync(ExecutionOptions options)
        {
            return graphQLService.ExecuteAsync(options);
        }
    }
}
