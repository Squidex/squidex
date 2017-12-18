// ==========================================================================
//  IResultList.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Infrastructure
{
    public interface IResultList<T> : IReadOnlyList<T>
    {
        long Total { get; }
    }
}
