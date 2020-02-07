// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class UpdateIndexEntry : IIndexCommand
    {
        public string DocId { get; set; }

        public bool ServeAll { get; set; }

        public bool ServePublished { get; set; }
    }
}
