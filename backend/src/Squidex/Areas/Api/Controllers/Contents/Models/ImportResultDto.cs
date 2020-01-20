// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public sealed class ImportResultDto
    {
        /// <summary>
        /// The error when the import failed.
        /// </summary>
        public ErrorDto? Error { get; set; }

        /// <summary>
        /// The id of the content when the import succeeds.
        /// </summary>
        public Guid? ContentId { get; set; }

        public static ImportResultDto FromImportResult(ImportResultItem result, HttpContext httpContext)
        {
            return new ImportResultDto { ContentId = result.ContentId, Error = result.Exception?.ToErrorDto(httpContext) };
        }
    }
}
