// ==========================================================================
//  ITenantEntity.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Read.Schemas.Repositories
{
    public interface ITenantEntity : IEntity
    {
        Guid TenantId { get; set; }
    }
}