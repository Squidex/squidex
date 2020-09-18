// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Commands
{
    public sealed class UpsertContent : ContentDataCommand, ISchemaCommand
    {
        public bool Publish { get; set; }

        public UpsertContent()
        {
            ContentId = DomainId.NewGuid();
        }
    }
}
