// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class SearchContext
    {
        public SearchScope Scope { get; set; }

        public HashSet<string> Languages { get; set; }
    }
}
