// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Teams.Commands
{
    public sealed class CreateTeam : TeamCommand
    {
        public DomainId TeamId { get; set; }

        public string Name { get; set; }

        public override DomainId AggregateId
        {
            get => TeamId;
        }

        public CreateTeam()
        {
            TeamId = DomainId.NewGuid();
        }
    }
}
