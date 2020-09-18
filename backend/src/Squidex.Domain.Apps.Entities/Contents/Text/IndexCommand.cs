// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public abstract class IndexCommand
    {
        public NamedId<Guid> AppId { get; set; }

        public NamedId<Guid> SchemaId { get; set; }

        public string DocId { get; set; }
    }
}
