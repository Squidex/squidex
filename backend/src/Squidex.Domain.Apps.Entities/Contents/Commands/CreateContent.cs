// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Commands
{
    public sealed class CreateContent : ContentDataCommand, ISchemaCommand
    {
        public CreateContent()
        {
            ContentId = DomainId.NewGuid();
        }
    }
}
