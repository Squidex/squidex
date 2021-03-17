// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

#pragma warning disable CA1822 // Mark members as static

namespace Squidex.Domain.Apps.Core.Rules.EnrichedEvents
{
    public abstract class EnrichedUserEventBase : EnrichedEvent
    {
        public RefToken Actor { get; set; }

        [IgnoreDataMember]
        public IUser? User { get; set; }

        public bool ShouldSerializeUser()
        {
            return false;
        }
    }
}
