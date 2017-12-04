// ==========================================================================
//  AssetState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.ValidateContent;

namespace Squidex.Domain.Apps.Entities.Assets.State
{
    public sealed class AssetState : DomainObjectState<AssetState>,
        IAssetEntity,
        IAssetInfo,
        IUpdateableEntityWithAppRef
    {
        [JsonProperty]
        public Guid AppId { get; set; }

        [JsonProperty]
        public string FileName { get; set; }

        [JsonProperty]
        public string MimeType { get; set; }

        [JsonProperty]
        public long FileVersion { get; set; }

        [JsonProperty]
        public long FileSize { get; set; }

        [JsonProperty]
        public long TotalSize { get; set; }

        [JsonProperty]
        public bool IsImage { get; set; }

        [JsonProperty]
        public int? PixelWidth { get; set; }

        [JsonProperty]
        public int? PixelHeight { get; set; }

        [JsonProperty]
        public bool IsDeleted { get; set; }

        Guid IAssetInfo.AssetId
        {
            get { return Id; }
        }
    }
}
