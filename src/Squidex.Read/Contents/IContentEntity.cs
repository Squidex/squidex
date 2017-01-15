// ==========================================================================
//  IContentEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Contents;

namespace Squidex.Read.Contents
{
    public interface IContentEntity : IEntity
    {
        bool IsPublished { get; }

        ContentData Data { get; }
    }
}
