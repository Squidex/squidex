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
        Guid Id { get; set; }

        Instant Created { get; set; }

        Instant LastModified { get; set; }
    }
}