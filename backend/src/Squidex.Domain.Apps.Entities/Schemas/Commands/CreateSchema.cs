// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public sealed class CreateSchema : UpsertCommand, IAggregateCommand
    {
        public DomainId SchemaId { get; set; }

        public string Name { get; set; }

        public bool IsSingleton { get; set; }

        public override DomainId AggregateId
        {
            get { return DomainId.Combine(AppId, SchemaId); }
        }

        public CreateSchema()
        {
            SchemaId = DomainId.NewGuid();
        }

        public Schema ToSchema()
        {
            return ToSchema(Name, IsSingleton);
        }
    }
}