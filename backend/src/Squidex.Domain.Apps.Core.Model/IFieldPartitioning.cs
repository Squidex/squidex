// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Core
{
    public interface IFieldPartitioning : IReadOnlyCollection<IFieldPartitionItem>
    {
        IFieldPartitionItem Master { get; }

        bool TryGetItem(string key, out IFieldPartitionItem item);
    }
}
