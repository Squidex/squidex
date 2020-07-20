// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public sealed class BulkResultDto
    {
        /// <summary>
        /// The error when the import failed.
        /// </summary>
        public ErrorDto? Error { get; set; }

        /// <summary>
        /// The id of the content when the import succeeds.
        /// </summary>
        public DomainId? ContentId { get; set; }

        public static BulkResultDto FromImportResult(BulkUpdateResultItem result, HttpContext httpContext)
        {
            var error = result.Exception?.ToErrorDto(httpContext).Error;

            return new BulkResultDto { ContentId = result.ContentId, Error = error };
        }
    }
}
