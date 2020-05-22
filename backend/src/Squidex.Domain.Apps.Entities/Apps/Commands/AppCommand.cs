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
    public abstract class AppCommand : SquidexCommand, IAggregateCommand
    {
        public DomainId AppId { get; set; }

        DomainId IAggregateCommand.AggregateId
        {
            get { return AppId; }
        }
    }
}
