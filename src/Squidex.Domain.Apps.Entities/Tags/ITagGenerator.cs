// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Entities.Tags
{
    public interface ITagGenerator<in T>
    {
        void GenerateTags(T source, HashSet<string> tags);
    }
}
