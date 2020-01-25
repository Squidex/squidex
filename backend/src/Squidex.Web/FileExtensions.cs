// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure.Assets;

namespace Squidex.Web
{
    public static class FileExtensions
    {
        public static AssetFile ToAssetFile(this IFormFile formFile)
        {
            if (string.IsNullOrWhiteSpace(formFile.ContentType))
            {
                throw new ValidationException("File content-type is not defined.");
            }

            if (string.IsNullOrWhiteSpace(formFile.FileName))
            {
                throw new ValidationException("File name is not defined.");
            }

            return new AssetFile(formFile.FileName, formFile.ContentType, formFile.Length, formFile.OpenReadStream);
        }
    }
}
