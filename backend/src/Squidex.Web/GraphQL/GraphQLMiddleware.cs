// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Server.Transports.AspNetCore.Common;
using Microsoft.AspNetCore.Http;

namespace Squidex.Web.GraphQL
{
    public sealed class GraphQLMiddleware : GraphQLHttpMiddleware<DummySchema>
    {
        private static readonly RequestDelegate Noop = _ => Task.CompletedTask;

        public GraphQLMiddleware(IGraphQLRequestDeserializer deserializer)
            : base(Noop, default, deserializer)
        {
        }
    }
}
