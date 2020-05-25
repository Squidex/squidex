// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets.Commands
{
    public abstract class AssetCommand : AppCommandBase, IAggregateCommand
    {
        public DomainId AssetId { get; set; }

        public override DomainId AggregateId
        {
            get { return DomainId.Combine(AppId, AssetId); }
        }
    }
}
