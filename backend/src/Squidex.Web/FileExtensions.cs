// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Squidex.Assets;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Web;

public static class FileExtensions
{
    public static AssetFile ToAssetFile(this IFormFile formFile)
    {
        if (string.IsNullOrWhiteSpace(formFile.ContentType))
        {
            throw new ValidationException(T.Get("common.httpContentTypeNotDefined"));
        }

        if (string.IsNullOrWhiteSpace(formFile.FileName))
        {
            throw new ValidationException(T.Get("common.httpFileNameNotDefined"));
        }

        return new DelegateAssetFile(formFile.FileName, formFile.ContentType, formFile.Length, formFile.OpenReadStream);
    }
}
