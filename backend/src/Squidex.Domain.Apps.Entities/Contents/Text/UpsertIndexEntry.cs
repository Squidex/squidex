// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class UpsertIndexEntry : IndexCommand
    {
        public Dictionary<string, string> Texts { get; set; }

        public bool ServeAll { get; set; }

        public bool ServePublished { get; set; }

        public DomainId ContentId { get; set; }
    }
}
