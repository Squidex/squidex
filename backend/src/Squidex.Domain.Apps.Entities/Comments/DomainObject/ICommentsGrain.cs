// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Comments.DomainObject
{
    public interface ICommentsGrain : IGrainWithStringKey
    {
        Task<J<object>> ExecuteAsync(J<CommentsCommand> command);

        Task<CommentsResult> GetCommentsAsync(long sinceVersion = EtagVersion.Any);
    }
}
