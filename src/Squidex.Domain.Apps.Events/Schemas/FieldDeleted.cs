// ==========================================================================
//  FieldDeleted.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Schemas
{
    [TypeName("FieldDeletedEvent")]
    public sealed class FieldDeleted : FieldEvent
    {
    }
}
