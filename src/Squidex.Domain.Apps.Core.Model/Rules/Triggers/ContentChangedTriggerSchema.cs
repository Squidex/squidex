// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Core.Rules.Triggers
{
    public sealed class ContentChangedTriggerSchema : Freezable
    {
        public Guid SchemaId { get; set; }

        public bool SendCreate { get; set; }

        public bool SendUpdate { get; set; }

        public bool SendDelete { get; set; }

        public bool SendPublish { get; set; }

        public bool SendUnpublish { get; set; }

        public bool SendArchived { get; set; }

        public bool SendRestore { get; set; }
    }
}
