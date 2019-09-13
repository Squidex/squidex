// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Infrastructure.Orleans.Indexes
{
    public class UniqueNameIndexState<T>
    {
        public Dictionary<string, T> Names { get; set; } = new Dictionary<string, T>();
    }
}
