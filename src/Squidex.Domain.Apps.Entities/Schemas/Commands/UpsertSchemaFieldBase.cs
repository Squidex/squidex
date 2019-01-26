// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public abstract class UpsertSchemaFieldBase
    {
        public string Name { get; set; }

        public bool IsLocked { get; set; }

        public bool IsHidden { get; set; }

        public bool IsDisabled { get; set; }

        public FieldProperties Properties { get; set; }
    }
}
