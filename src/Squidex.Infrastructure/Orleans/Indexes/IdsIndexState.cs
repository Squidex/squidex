// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Infrastructure.Orleans.Indexes
{
    public class IdsIndexState<T>
    {
        public HashSet<T> Ids { get; set; } = new HashSet<T>();
    }
}
