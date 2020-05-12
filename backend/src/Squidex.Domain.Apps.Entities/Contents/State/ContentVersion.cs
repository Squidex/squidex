// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.State
{
    public sealed class ContentVersion
    {
        public Status Status { get; }

        public NamedContentData Data { get; }

        public ContentVersion(Status status, NamedContentData data)
        {
            Guard.NotNull(data, nameof(data));

            Status = status;

            Data = data;
        }

        public ContentVersion WithStatus(Status status)
        {
            return new ContentVersion(status, Data);
        }

        public ContentVersion WithData(NamedContentData data)
        {
            return new ContentVersion(Status, data);
        }
    }
}
