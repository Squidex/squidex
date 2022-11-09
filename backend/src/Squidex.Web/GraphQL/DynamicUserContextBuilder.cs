// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Server.Transports.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents.GraphQL;

namespace Squidex.Web.GraphQL;

public sealed class DynamicUserContextBuilder : IUserContextBuilder
{
    private readonly ObjectFactory factory = ActivatorUtilities.CreateFactory(typeof(GraphQLExecutionContext), new[] { typeof(Context) });

    public ValueTask<IDictionary<string, object?>?> BuildUserContextAsync(HttpContext context, object? payload)
    {
        var executionContext = (GraphQLExecutionContext)factory(context.RequestServices, new object[] { context.Context() });

        return new ValueTask<IDictionary<string, object?>?>(executionContext);
    }
}
