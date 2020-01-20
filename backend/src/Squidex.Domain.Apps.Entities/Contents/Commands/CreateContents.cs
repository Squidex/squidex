// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Commands
{
    public sealed class CreateContents : SquidexCommand, ISchemaCommand, IAppCommand
    {
        public NamedId<Guid> AppId { get; set; }

        public NamedId<Guid> SchemaId { get; set; }

        public bool Publish { get; set; }

        public bool DoNotValidate { get; set; }

        public bool DoNotScript { get; set; }

        public bool OptimizeValidation { get; set; }

        public List<NamedContentData> Datas { get; set; }
    }
}
