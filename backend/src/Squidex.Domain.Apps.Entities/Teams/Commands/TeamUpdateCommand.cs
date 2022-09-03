// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Teams.Commands
{
    public abstract class TeamUpdateCommand : TeamCommand, ITeamCommand
    {
        public DomainId TeamId { get; set; }

        public override DomainId AggregateId
        {
            get => TeamId;
        }
    }
}
