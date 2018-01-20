// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Events.Contents
{
    public abstract class ContentEvent : SchemaEvent
    {
        public Guid ContentId { get; set; }
    }
}
