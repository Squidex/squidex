// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class CreateApp : AppCommand, IAggregateCommand
    {
        public DomainId AppId { get; set; }

        public string Name { get; set; }

        public string? Template { get; set; }

        public override DomainId AggregateId
        {
            get { return AppId; }
        }

        public CreateApp()
        {
            AppId = DomainId.NewGuid();
        }
    }
}