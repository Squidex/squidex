// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Core
{
    public interface IFieldPartitionItem
    {
        string Key { get; }

        string Name { get; }

        bool IsOptional { get; }

        IEnumerable<string> Fallback { get; }
    }
}
