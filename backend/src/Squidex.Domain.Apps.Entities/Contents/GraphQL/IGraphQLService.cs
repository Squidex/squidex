// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public interface IGraphQLService
    {
        Task<ExecutionResult> ExecuteAsync(ExecutionOptions options);
    }
}
