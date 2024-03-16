// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Contents.Commands
{
    public sealed class CreateContent : ContentDataCommand, ISchemaCommand
    {
        public Status? Status { get; set; }

        public CreateContent()
        {
            ContentId = DomainId.NewGuid();
        }

        public ChangeContentStatus AsChange(Status status)
        {
            return SimpleMapper.Map(this, new ChangeContentStatus { Status = status });
        }
    }
}
