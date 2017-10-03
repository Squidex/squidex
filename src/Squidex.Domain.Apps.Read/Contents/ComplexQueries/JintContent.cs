// ==========================================================================
//  JintContent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Jint;
using Jint.Native.Object;
using Squidex.Domain.Apps.Core.Scripting.ContentWrapper;

namespace Squidex.Domain.Apps.Read.Contents.ComplexQueries
{
    public sealed class JintContent : ObjectInstance
    {
        public JintContent(Engine engine, IContentEntity content)
            : base(engine)
        {
            FastAddProperty("id", content.Id.ToString(), true, false, false);
            FastAddProperty("status", content.Status.ToString(), true, false, false);
            FastAddProperty("created", content.Created.ToString(), true, false, false);
            FastAddProperty("createdBy", content.CreatedBy.ToString(), true, false, false);
            FastAddProperty("lastModified", content.LastModified.ToString(), true, false, false);
            FastAddProperty("lastModifiedBy", content.LastModifiedBy.ToString(), true, false, false);
            FastAddProperty("version", content.Version, true, false, false);
            FastAddProperty("data", new ContentDataObject(engine, content.Data), true, false, false);
        }
    }
}
