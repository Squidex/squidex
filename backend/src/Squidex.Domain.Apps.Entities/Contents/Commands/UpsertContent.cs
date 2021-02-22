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
    public sealed class UpsertContent : ContentDataCommand, ISchemaCommand
    {
        public Status? Status { get; set; }

        public bool CheckReferrers { get; set; }

        public UpsertContent()
        {
            ContentId = DomainId.NewGuid();
        }

        public CreateContent AsCreate()
        {
            return SimpleMapper.Map(this, new CreateContent());
        }

        public UpdateContent AsUpdate()
        {
            return SimpleMapper.Map(this, new UpdateContent());
        }

        public ChangeContentStatus AsChange(Status status)
        {
            return SimpleMapper.Map(this, new ChangeContentStatus { Status = status });
        }
    }
}
