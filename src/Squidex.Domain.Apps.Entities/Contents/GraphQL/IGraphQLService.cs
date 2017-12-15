// ==========================================================================
//  IGraphQLService.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Security.Claims;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public interface IGraphQLService
    {
        Task<(object Data, object[] Errors)> QueryAsync(IAppEntity app, ClaimsPrincipal user, GraphQLQuery query);
    }
}
