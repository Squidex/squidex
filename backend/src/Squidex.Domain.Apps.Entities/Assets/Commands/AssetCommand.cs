// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Commands;

public abstract class AssetCommand : AssetCommandBase
{
    public DomainId AssetId { get; set; }

    public bool DoNotScript { get; set; }

    public override DomainId AggregateId
    {
        get => DomainId.Combine(AppId, AssetId);
    }
}
