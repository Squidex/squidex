// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events
{
    public abstract class AppEvent : SquidexEvent
    {
        public NamedId<DomainId> AppId { get; set; }
    }
}
