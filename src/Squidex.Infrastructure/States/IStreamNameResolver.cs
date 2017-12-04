// ==========================================================================
//  IStreamNameResolver.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.States
{
    public interface IStreamNameResolver
    {
        string GetStreamName(Type aggregateType, string id);
    }
}
