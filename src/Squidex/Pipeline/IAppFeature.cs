// ==========================================================================
//  IAppFeature.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Pipeline
{
    public interface IAppFeature
    {
        Guid AppId { get; }
    }
}
