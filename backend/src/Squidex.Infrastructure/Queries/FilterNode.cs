// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Infrastructure.Queries
{
    public abstract class FilterNode<TValue>
    {
        public abstract T Accept<T>(FilterNodeVisitor<T, TValue> visitor);

        public abstract void AddFields(HashSet<string> fields);

        public abstract override string ToString();
    }
}
