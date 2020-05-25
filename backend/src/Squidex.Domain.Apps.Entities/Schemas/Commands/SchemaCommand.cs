// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public abstract class SchemaCommand : SquidexCommand, IAppCommand, IAggregateCommand
    {
        public NamedId<DomainId> AppId { get; set; }

        public abstract DomainId AggregateId { get; }
    }
}
