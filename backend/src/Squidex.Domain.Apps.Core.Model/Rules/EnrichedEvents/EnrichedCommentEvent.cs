// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Core.Rules.EnrichedEvents
{
    public sealed class EnrichedCommentEvent : EnrichedUserEventBase
    {
        public string Text { get; set; }

        [IgnoreDataMember]
        public IUser MentionedUser { get; set; }

        public override long Partition
        {
            get { return MentionedUser.Id.GetHashCode(); }
        }
    }
}
