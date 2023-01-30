// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Server.Transports.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Squidex.Web.GraphQL;

public sealed class GraphQLRunner
{
    private readonly GraphQLHttpMiddleware<DummySchema> middleware;

    public GraphQLRunner(IServiceProvider serviceProvider)
    {
        RequestDelegate next = x => Task.CompletedTask;

        var options = new GraphQLHttpMiddlewareOptions
        {
            DefaultResponseContentType = new MediaTypeHeaderValue("application/json")
        };

        middleware = ActivatorUtilities.CreateInstance<GraphQLHttpMiddleware<DummySchema>>(serviceProvider, next, options);
    }

    public Task InvokeAsync(HttpContext context)
    {
        return middleware.InvokeAsync(context);
    }
}
