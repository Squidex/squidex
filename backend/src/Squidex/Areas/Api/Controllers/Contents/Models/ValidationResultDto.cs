// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public class ValidationResultDto
    {
        /// <summary>
        /// The validation errors.
        /// </summary>
        public string[] Errors { get; set; }

        public static ValidationResultDto FromResult(ValidationResult result)
        {
            return new ValidationResultDto
            {
                Errors = ApiExceptionConverter.ToErrors(result.Errors).ToArray()
            };
        }
    }
}
