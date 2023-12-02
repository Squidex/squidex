// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Squidex.Infrastructure.Commands;

public record Entity
{
    public DomainId UniqueId { get; init; }

    public DomainId Id { get; init; }

    public RefToken CreatedBy { get; init; }

    public RefToken LastModifiedBy { get; init; }

    public Instant Created { get; init; }

    public Instant LastModified { get; init; }

    public long Version { get; init; }
}
