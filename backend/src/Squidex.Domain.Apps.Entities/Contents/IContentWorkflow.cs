// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents;

public interface IContentWorkflow
{
    ValueTask<Status> GetInitialStatusAsync(Schema schema);

    ValueTask<bool> CanMoveToAsync(Content content, Status status, Status next, ClaimsPrincipal? user);

    ValueTask<bool> CanUpdateAsync(Content content, Status status, ClaimsPrincipal? user);

    ValueTask<bool> CanPublishInitialAsync(Schema schema, ClaimsPrincipal? user);

    ValueTask<bool> ShouldValidateAsync(Schema schema, Status status);

    ValueTask<StatusInfo?> GetInfoAsync(Content content, Status status);

    ValueTask<StatusInfo[]> GetNextAsync(Content content, Status status, ClaimsPrincipal? user);

    ValueTask<StatusInfo[]> GetAllAsync(Schema schema);
}
