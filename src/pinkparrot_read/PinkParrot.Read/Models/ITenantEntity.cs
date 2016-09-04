// ==========================================================================
//  ITenantEntity.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Read.Models
{
    public interface ITenantEntity
    {
        Guid TenantId { get; set; }
    }
}