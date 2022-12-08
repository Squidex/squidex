// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Entities.Backup;

public sealed record BackupRestore(RefToken Actor, Uri Url, string? NewAppName = null);

public sealed record BackupStart(DomainId AppId, RefToken Actor);

public sealed record BackupDelete(DomainId AppId, DomainId Id);

public sealed record BackupClear(DomainId AppId);
