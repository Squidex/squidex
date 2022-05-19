// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Server.Transports.AspNetCore;
using Microsoft.AspNetCore.Http;

namespace Squidex.Web.GraphQL
{
    public sealed class GraphQLRunner
    {
        private readonly GraphQLHttpMiddleware<DummySchema> middleware;

        public GraphQLRunner(IGraphQLTextSerializer deserializer)
        {
            middleware = new GraphQLHttpMiddleware<DummySchema>(deserializer);
        }

        public Task InvokeAsync(HttpContext context)
        {
            return middleware.InvokeAsync(context, x => Task.CompletedTask);
        }
    }
}
