// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities
{
    public interface IEntity
    {
        DomainId Id { get;  }

        Instant Created { get; }

        Instant LastModified { get; }

        DomainId UniqueId { get; }
    }
}