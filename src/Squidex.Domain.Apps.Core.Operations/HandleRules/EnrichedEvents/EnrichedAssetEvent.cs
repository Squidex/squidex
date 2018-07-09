// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents
{
    public sealed class EnrichedAssetEvent : EnrichedEvent
    {
        public EnrichedAssetEventType Type { get; set; }

        public Guid Id { get; set; }

        public Instant Created { get; set; }

        public Instant LastModified { get; set; }

        public RefToken CreatedBy { get; set; }

        public RefToken LastModifiedBy { get; set; }

        public string MimeType { get; set; }

        public string FileName { get; set; }

        public long FileVersion { get; set; }

        public long FileSize { get; set; }

        public bool IsImage { get; set; }

        public int? PixelWidth { get; set; }

        public int? PixelHeight { get; set; }

        public override Guid AggregateId
        {
            get { return Id; }
        }
    }
}
