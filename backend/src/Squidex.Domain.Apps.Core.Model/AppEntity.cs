// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Core;

public record AppEntity : Entity
{
    public NamedId<DomainId> AppId { get; init; }

    public bool IsDeleted { get; init; }

    public override DomainId UniqueId
    {
        get => DomainId.Combine(AppId?.Id ?? default, Id);
    }
}
