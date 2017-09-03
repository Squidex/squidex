// ==========================================================================
//  ContentChangedResult.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Domain.Apps.Write.Contents
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
