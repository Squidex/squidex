// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL.Server.Transports.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents.GraphQL;

namespace Squidex.Web.GraphQL
{
    public sealed class DynamicUserContextBuilder : IUserContextBuilder
    {
        private readonly ObjectFactory factory = ActivatorUtilities.CreateFactory(typeof(GraphQLExecutionContext), new[] { typeof(Context) });

        public Task<IDictionary<string, object>> BuildUserContext(HttpContext httpContext)
        {
            var executionContext = (GraphQLExecutionContext)factory(httpContext.RequestServices, new object[] { httpContext.Context() });

            return Task.FromResult<IDictionary<string, object>>(executionContext);
        }
    }
}
