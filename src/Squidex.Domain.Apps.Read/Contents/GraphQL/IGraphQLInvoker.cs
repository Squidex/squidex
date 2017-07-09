// ==========================================================================
//  IGraphQLInvoker.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Read.Apps;

namespace Squidex.Domain.Apps.Read.Contents.GraphQL
{
    public interface IGraphQLInvoker
    {
        Task<object> QueryAsync(IAppEntity app, GraphQLQuery query);
    }
}
