// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents;

public interface IContentWorkflow
{
    ValueTask<Status> GetInitialStatusAsync(ISchemaEntity schema);

    ValueTask<bool> CanMoveToAsync(ISchemaEntity schema, Status status, Status next, ContentData data, ClaimsPrincipal? user);

    ValueTask<bool> CanMoveToAsync(IContentEntity content, Status status, Status next, ClaimsPrincipal? user);

    ValueTask<bool> CanUpdateAsync(IContentEntity content, Status status, ClaimsPrincipal? user);

    ValueTask<bool> CanPublishInitialAsync(ISchemaEntity schema, ClaimsPrincipal? user);

    ValueTask<bool> ShouldValidateAsync(ISchemaEntity schema, Status status);

    ValueTask<StatusInfo?> GetInfoAsync(IContentEntity content, Status status);

    ValueTask<StatusInfo[]> GetNextAsync(IContentEntity content, Status status, ClaimsPrincipal? user);

    ValueTask<StatusInfo[]> GetAllAsync(ISchemaEntity schema);
}
