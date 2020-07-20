﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Commands
{
    public sealed class BulkUpdateContents : SquidexCommand, IAppCommand, ISchemaCommand
    {
        public NamedId<DomainId> AppId { get; set; }

        public NamedId<DomainId> SchemaId { get; set; }

        public bool Publish { get; set; }

        public bool DoNotValidate { get; set; }

        public bool DoNotScript { get; set; }

        public bool OptimizeValidation { get; set; }

        public List<BulkUpdateJob>? Jobs { get; set; }
    }
}
