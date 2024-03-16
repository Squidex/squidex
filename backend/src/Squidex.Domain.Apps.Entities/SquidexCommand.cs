// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities
{
    public abstract class SquidexCommand : ICommand
    {
        public RefToken Actor { get; set; }

        public ClaimsPrincipal? User { get; set; }

        public bool FromRule { get; set; }

        public long ExpectedVersion { get; set; } = EtagVersion.Auto;
    }
}
