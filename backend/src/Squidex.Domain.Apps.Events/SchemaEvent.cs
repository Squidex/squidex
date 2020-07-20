// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events
{
    public abstract class SchemaEvent : AppEvent
    {
        public NamedId<DomainId> SchemaId { get; set; }
    }
}
