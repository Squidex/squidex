// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure.Assets;

namespace Squidex.Web
{
    public static class FileExtensions
    {
        public static AssetFile ToAssetFile(this IFormFile formFile)
        {
            return new AssetFile(formFile.FileName, formFile.ContentType, formFile.Length, formFile.OpenReadStream);
        }
    }
}
