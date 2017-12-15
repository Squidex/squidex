// ==========================================================================
//  FakeContentEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.TestData
{
    public sealed class FakeContentEntity : IContentEntity
    {
        public Guid Id { get; set; }

        public Guid AppId { get; set; }

        public long Version { get; set; }

        public Instant Created { get; set; }

        public Instant LastModified { get; set; }

        public RefToken CreatedBy { get; set; }

        public RefToken LastModifiedBy { get; set; }

        public NamedContentData Data { get; set; }

        public Status Status { get; set; }
    }
}
