// ==========================================================================
//  IContentEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json.Linq;

namespace Squidex.Read.Contents
{
    public interface IContentEntity : IEntity
    {
        JToken Data { get; }
    }
}
