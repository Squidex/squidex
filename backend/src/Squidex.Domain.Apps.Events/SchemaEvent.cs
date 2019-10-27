// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events
{
    public abstract class SchemaEvent : AppEvent
    {
        public NamedId<Guid> SchemaId { get; set; }
    }
}
