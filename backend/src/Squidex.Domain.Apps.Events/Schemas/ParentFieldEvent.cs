// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Schemas;

public abstract class ParentFieldEvent : SchemaEvent
{
    public NamedId<long>? ParentFieldId { get; set; }
}
