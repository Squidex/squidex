// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Comments
{
    public sealed class Comment
    {
        public DomainId Id { get; }

        public Instant Time { get; }

        public RefToken User { get; }

        public string Text { get; }

        public Uri? Url { get; }

        public Comment(DomainId id, Instant time, RefToken user, string text, Uri? url = null)
        {
            Guard.NotEmpty(id, nameof(id));
            Guard.NotNull(user, nameof(user));
            Guard.NotNull(text, nameof(text));

            Id = id;
            Text = text;
            Time = time;
            User = user;
            Url = url;
        }
    }
}
