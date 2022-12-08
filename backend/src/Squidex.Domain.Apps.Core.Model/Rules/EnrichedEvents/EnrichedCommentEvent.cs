// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using Squidex.Shared.Users;

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable SA1133 // Do not combine attributes

namespace Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

public sealed class EnrichedCommentEvent : EnrichedUserEventBase
{
    [FieldDescription(nameof(FieldDescriptions.CommentText))]
    public string Text { get; set; }

    [FieldDescription(nameof(FieldDescriptions.CommentUrl))]
    public Uri? Url { get; set; }

    [FieldDescription(nameof(FieldDescriptions.CommentMentionedUser)), JsonIgnore]
    public IUser MentionedUser { get; set; }

    [JsonIgnore]
    public override long Partition
    {
        get => MentionedUser?.Id.GetHashCode(StringComparison.Ordinal) ?? 0;
    }

    public bool ShouldSerializeMentionedUser()
    {
        return false;
    }

    public bool ShouldSerializePartition()
    {
        return false;
    }
}
