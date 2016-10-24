// ==========================================================================
//  IAppFeature.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Pipeline
{
    public interface IAppFeature
    {
        Guid AppId { get; }
    }
}
