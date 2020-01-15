// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Core.Rules.EnrichedEvents
{
    public abstract class EnrichedUserEventBase : EnrichedEvent
    {
        public RefToken Actor { get; set; }

        [IgnoreDataMember]
        public IUser? User { get; set; }
    }
}
