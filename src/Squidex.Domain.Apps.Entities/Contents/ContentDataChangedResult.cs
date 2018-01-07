// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentDataChangedResult : EntitySavedResult
    {
        public NamedContentData Data { get; }

        public ContentDataChangedResult(NamedContentData data, long version)
            : base(version)
        {
            Data = data;
        }
    }
}
