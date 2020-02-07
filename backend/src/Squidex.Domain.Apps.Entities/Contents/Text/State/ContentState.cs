// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Entities.Contents.Text.State
{
    public sealed class ContentState
    {
        public Guid ContentId { get; set; }

        public string DocIdCurrent { get; set; }

        public string? DocIdNew { get; set; }

        public string? DocIdForPublished { get; set; }
    }
}
