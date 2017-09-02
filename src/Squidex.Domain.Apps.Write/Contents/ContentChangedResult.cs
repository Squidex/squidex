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
    public sealed class ContentChangedResult : EntitySavedResult
    {
        public NamedContentData Content { get; }

        public ContentChangedResult(NamedContentData content, long version) 
            : base(version)
        {
            Content = content;
        }
    }
}
