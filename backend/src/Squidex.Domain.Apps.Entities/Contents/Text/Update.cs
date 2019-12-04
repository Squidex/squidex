// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Orleans.Concurrency;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    [Immutable]
    public sealed class Update
    {
        public Guid Id { get; set; }

        public Dictionary<string, string> Text { get; set; }

        public bool OnlyDraft { get; set; }
    }
}
