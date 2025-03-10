﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Events;

namespace Squidex.Domain.Apps.Entities.Backup;

public interface IBackupWriter : IDisposable
{
    int WrittenAttachments { get; }

    int WrittenEvents { get; }

    Task<Stream> OpenBlobAsync(string name,
        CancellationToken ct = default);

    void WriteEvent(StoredEvent storedEvent,
        CancellationToken ct = default);

    Task WriteJsonAsync(string name, object value,
        CancellationToken ct = default);
}
