// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Entities.Jobs;

public sealed record JobStart(DomainId OwnerId, JobRequest Request) : JobMessage(OwnerId);

public sealed record JobCancel(DomainId OwnerId, string? TaskName) : JobMessage(OwnerId);

public sealed record JobDelete(DomainId OwnerId, DomainId JobId) : JobMessage(OwnerId);

public sealed record JobClear(DomainId OwnerId) : JobMessage(OwnerId);

public sealed record JobWakeup(DomainId OwnerId) : JobMessage(OwnerId);

public abstract record JobMessage(DomainId OwnerId);
