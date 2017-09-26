// ==========================================================================
//  IContentVersionLoader.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Write.Contents
{
    public interface IContentVersionLoader
    {
        Task<NamedContentData> LoadAsync(Guid appId, Guid id, long version);
    }
}
