// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed class EFContentTableEntity
{
    public long Id { get; set; }

    public DomainId AppId { get; set; }

    public DomainId SchemaId { get; set; }
}
