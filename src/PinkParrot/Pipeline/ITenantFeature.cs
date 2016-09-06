// ==========================================================================
//  ITenantFeature.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Pipeline
{
    public interface ITenantFeature
    {
        Guid TenantId { get; }
    }
}
