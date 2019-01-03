// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Core.Rules.Triggers
{
    public sealed class ContentChangedTriggerSchemaV2 : Freezable
    {
        public Guid SchemaId { get; set; }

        public string Condition { get; set; }
    }
}
