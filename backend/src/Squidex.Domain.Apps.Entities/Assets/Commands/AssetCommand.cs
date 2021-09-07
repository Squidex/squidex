// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets.Commands
{
    public abstract class AssetCommand : SquidexCommand, IAppCommand, IAggregateCommand
    {
        public NamedId<DomainId> AppId { get; set; }

        public DomainId AssetId { get; set; }

        public bool DoNotScript { get; set; }

        [IgnoreDataMember]
        public DomainId AggregateId
        {
            get => DomainId.Combine(AppId, AssetId);
        }
    }
}
