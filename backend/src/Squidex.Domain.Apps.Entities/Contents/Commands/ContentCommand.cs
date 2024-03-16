// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents.Commands
{
    public abstract class ContentCommand : SquidexCommand, IAppCommand, ISchemaCommand, IAggregateCommand
    {
        public NamedId<DomainId> AppId { get; set; }

        public NamedId<DomainId> SchemaId { get; set; }

        public DomainId ContentId { get; set; }

        public bool DoNotScript { get; set; }

        [IgnoreDataMember]
        public DomainId AggregateId
        {
            get => DomainId.Combine(AppId, ContentId);
        }
    }
}
