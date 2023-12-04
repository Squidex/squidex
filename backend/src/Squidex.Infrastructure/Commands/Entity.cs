// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Squidex.Infrastructure.Commands;

public abstract record Entity
{
    public DomainId Id { get; init; }

    public RefToken CreatedBy { get; init; }

    public RefToken LastModifiedBy { get; init; }

    public Instant Created { get; init; }

    public Instant LastModified { get; init; }

    public long Version { get; init; } = EtagVersion.Empty;

    public virtual DomainId UniqueId
    {
        get => Id;
    }
}
