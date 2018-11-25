// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;
using NodaTime;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities
{
    public abstract class DomainObjectState<T> : Cloneable<T>,
        IDomainState,
        IEntity,
        IEntityWithCreatedBy,
        IEntityWithLastModifiedBy,
        IEntityWithVersion,
        IUpdateableEntity,
        IUpdateableEntityWithCreatedBy,
        IUpdateableEntityWithLastModifiedBy
        where T : Cloneable
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public RefToken CreatedBy { get; set; }

        [DataMember]
        public RefToken LastModifiedBy { get; set; }

        [DataMember]
        public Instant Created { get; set; }

        [DataMember]
        public Instant LastModified { get; set; }

        [DataMember]
        public long Version { get; set; } = EtagVersion.Empty;

        public T Clone()
        {
            return Clone(x => { });
        }
    }
}
