// ==========================================================================
//  IFieldPartitionItem.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Core
{
    public interface IFieldPartitionItem
    {
        string Key { get; }

        string Name { get; }

        bool IsOptional { get; }

        IEnumerable<string> Fallback { get; }
    }
}
