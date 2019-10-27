// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public sealed class ReorderFields : ParentFieldCommand
    {
        public List<long> FieldIds { get; set; }
    }
}
