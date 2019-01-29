// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public sealed class AddField : ParentFieldCommand
    {
        public string Name { get; set; }

        public string Partitioning { get; set; }

        public FieldProperties Properties { get; set; }
    }
}