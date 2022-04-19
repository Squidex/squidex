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
#pragma warning disable SA1133 // Do not combine attributes

namespace Squidex.Domain.Apps.Core.Rules.EnrichedEvents
{
    public abstract class EnrichedUserEventBase : EnrichedEvent
    {
        [FieldDescription(nameof(FieldDescriptions.Actor))]
        public RefToken Actor { get; set; }

        [FieldDescription(nameof(FieldDescriptions.User)), IgnoreDataMember]
        public IUser? User { get; set; }

        public bool ShouldSerializeUser()
        {
            return false;
        }
    }
}
