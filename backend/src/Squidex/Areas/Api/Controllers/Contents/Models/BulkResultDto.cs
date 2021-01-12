﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public sealed class BulkResultDto
    {
        /// <summary>
        /// The error when the bulk job failed.
        /// </summary>
        public ErrorDto? Error { get; set; }

        /// <summary>
        /// The index of the bulk job where the result belongs to. The order can change.
        /// </summary>
        public int JobIndex { get; set; }

        /// <summary>
        /// The id of the content that has been handled successfully or not.
        /// </summary>
        public DomainId? ContentId { get; set; }

        public static BulkResultDto FromImportResult(BulkUpdateResultItem result, HttpContext httpContext)
        {
            var error = result.Exception?.ToErrorDto(httpContext).Error;

            return SimpleMapper.Map(result, new BulkResultDto { Error = error });
        }
    }
}
