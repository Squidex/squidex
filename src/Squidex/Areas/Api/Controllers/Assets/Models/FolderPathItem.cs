// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Entities.Assets;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class FolderPathItem
    {
        /// <summary>
        /// The id of the folder.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the folder.
        /// </summary>
        public string Name { get; set; }

        public static FolderPathItem FromAsset(IAssetEntity asset)
        {
            return new FolderPathItem { Id = asset.Id, Name = asset.Name };
        }
    }
}