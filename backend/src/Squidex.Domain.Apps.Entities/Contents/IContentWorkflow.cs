// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public interface IContentWorkflow
    {
        Task<StatusInfo> GetInitialStatusAsync(ISchemaEntity schema);

        Task<bool> CanPublishOnCreateAsync(ISchemaEntity schema, NamedContentData data, ClaimsPrincipal user);

        Task<bool> CanMoveToAsync(IContentInfo content, Status next, ClaimsPrincipal user);

        Task<bool> CanUpdateAsync(IContentInfo content, ClaimsPrincipal user);

        Task<StatusInfo> GetInfoAsync(IContentInfo content);

        Task<StatusInfo[]> GetNextsAsync(IContentInfo content, ClaimsPrincipal user);

        Task<StatusInfo[]> GetAllAsync(ISchemaEntity schema);
    }
}
