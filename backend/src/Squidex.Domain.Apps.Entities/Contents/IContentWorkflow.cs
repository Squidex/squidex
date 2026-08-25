// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.Contents;

public interface IContentWorkflow : IDisposable
{
    Status GetInitialStatus();

    bool CanMoveTo(Content content, Status status, Status next, ClaimsPrincipal? user);

    bool CanUpdate(Content content, Status status, ClaimsPrincipal? user);

    bool CanPublishInitial(ClaimsPrincipal? user);

    bool ShouldValidate(Status status);

    StatusInfo? GetInfo(Status status);

    StatusInfo[] GetNext(Content content, Status status, ClaimsPrincipal? user);

    StatusInfo[] GetAll();
}
