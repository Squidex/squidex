// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Squidex.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class UpsertAssetDto
    {
        /// <summary>
        /// The file to upload.
        /// </summary>
        public IFormFile File { get; set; }

        /// <summary>
        /// The optional parent folder id.
        /// </summary>
        [FromQuery]
        public DomainId ParentId { get; set; }

        /// <summary>
        /// True to duplicate the asset, event if the file has been uploaded.
        /// </summary>
        [FromQuery]
        public bool Duplicate { get; set; }

        public UpsertAsset ToCommand(DomainId id, AssetFile file)
        {
            return SimpleMapper.Map(this, new UpsertAsset { File = file, AssetId = id });
        }
    }
}
