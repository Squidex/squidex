// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
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
        [JsonProperty]
        public Guid Id { get; set; }

        [JsonProperty]
        public RefToken CreatedBy { get; set; }

        [JsonProperty]
        public RefToken LastModifiedBy { get; set; }

        [JsonProperty]
        public Instant Created { get; set; }

        [JsonProperty]
        public Instant LastModified { get; set; }

        [JsonProperty]
        public long Version { get; set; } = EtagVersion.Empty;

        public T Clone()
        {
            return Clone(x => { });
        }
    }
}
