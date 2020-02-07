// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class UpsertIndexEntry : IIndexCommand
    {
        public string DocId { get; set; }

        public Dictionary<string, string> Texts { get; set; }

        public bool ServeAll { get; set; }

        public bool ServePublished { get; set; }

        public Guid ContentId { get; set; }
    }
}
