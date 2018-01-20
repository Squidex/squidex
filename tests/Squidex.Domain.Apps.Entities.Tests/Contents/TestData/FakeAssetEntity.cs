﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.TestData
{
    public sealed class FakeAssetEntity : IAssetEntity
    {
        public Guid Id { get; set; }

        public Guid AppId { get; set; }

        public Guid AssetId { get; set; }

        public Instant Created { get; set; }

        public Instant LastModified { get; set; }

        public RefToken CreatedBy { get; set; }

        public RefToken LastModifiedBy { get; set; }

        public long Version { get; set; }

        public string MimeType { get; set; }

        public string FileName { get; set; }

        public long FileSize { get; set; }

        public long FileVersion { get; set; }

        public bool IsImage { get; set; }

        public bool IsDeleted { get; set; }

        public int? PixelWidth { get; set; }

        public int? PixelHeight { get; set; }
    }
}
