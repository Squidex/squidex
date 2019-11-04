// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Events.Schemas
{
    public sealed class SchemaUIFieldsSet : SchemaEvent
    {
        public FieldNames FieldsInLists { get; set; }

        public FieldNames FieldsInReferences { get; set; }
    }
}
