// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public interface IGraphQLService
    {
        Task<(object Data, object[] Errors)> QueryAsync(QueryContext context, GraphQLQuery query);
    }
}
