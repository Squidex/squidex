// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public abstract class ParentFieldCommand : SchemaUpdateCommand
    {
        public long? ParentFieldId { get; set; }
    }
}
