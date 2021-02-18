// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Commands
{
    public sealed class UpsertContent : UpdateContent, ISchemaCommand
    {
        public Status? Status { get; set; }

        public bool CheckReferrers { get; set; }

        public UpsertContent()
        {
            ContentId = DomainId.NewGuid();
        }
    }
}
