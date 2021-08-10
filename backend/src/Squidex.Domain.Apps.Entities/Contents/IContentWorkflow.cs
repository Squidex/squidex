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
        Task<Status> GetInitialStatusAsync(ISchemaEntity schema);

        Task<bool> CanMoveToAsync(ISchemaEntity schema, Status status, Status next, ContentData data, ClaimsPrincipal? user);

        Task<bool> CanMoveToAsync(IContentEntity content, Status status, Status next, ClaimsPrincipal? user);

        Task<bool> CanUpdateAsync(IContentEntity content, Status status, ClaimsPrincipal? user);

        Task<bool> CanPublishInitialAsync(ISchemaEntity schema, ClaimsPrincipal? user);

        Task<StatusInfo?> GetInfoAsync(IContentEntity content, Status status);

        Task<StatusInfo[]> GetNextAsync(IContentEntity content, Status status, ClaimsPrincipal? user);

        Task<StatusInfo[]> GetAllAsync(ISchemaEntity schema);
    }
}
