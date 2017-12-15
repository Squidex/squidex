// ==========================================================================
//  IEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using NodaTime;

namespace Squidex.Domain.Apps.Entities
{
    public interface IEntity
    {
        Guid Id { get; }

        Instant Created { get; }

        Instant LastModified { get; }
    }
}