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
        public bool IsDraft { get; set; }

        public long AppVersion { get; set; }

        public long SchemaVersion { get; set; }

        public List<string> AppLanguages { get; set; }
    }
}
