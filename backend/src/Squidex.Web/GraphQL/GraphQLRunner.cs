// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Server.Transports.AspNetCore;
using Microsoft.AspNetCore.Http;

namespace Squidex.Web.GraphQL
{
    public sealed class GraphQLRunner
    {
        private readonly GraphQLHttpMiddleware<DummySchema> middleware;

        public GraphQLRunner(IGraphQLRequestDeserializer deserializer)
        {
            middleware = new GraphQLHttpMiddleware<DummySchema>(x => Task.CompletedTask, deserializer);
        }

        public Task InvokeAsync(HttpContext context)
        {
            return middleware.InvokeAsync(context);
        }
    }
}
