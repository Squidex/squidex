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
    public abstract class AssetCommand : SquidexCommand, IAppCommand, IAggregateCommand
    {
        public NamedId<DomainId> AppId { get; set; }

        public DomainId AssetId { get; set; }

        public DomainId AggregateId
        {
            get { return DomainId.Combine(AppId, AssetId); }
        }
    }
}
