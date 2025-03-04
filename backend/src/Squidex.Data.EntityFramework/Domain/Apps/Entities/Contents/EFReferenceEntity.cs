// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed record EFReferenceCompleteEntity : EFReferenceEntity
{
}

public sealed record EFReferencePublishedEntity : EFReferenceEntity
{
}

public abstract record EFReferenceEntity
{
    public DomainId AppId { get; set; }

    public DomainId FromKey { get; set; }

    public DomainId FromSchema { get; set; }

    public DomainId ToId { get; set; }
}
