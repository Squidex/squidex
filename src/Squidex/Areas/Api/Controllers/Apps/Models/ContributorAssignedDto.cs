// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class ContributorAssignedDto
    {
        /// <summary>
        /// The id of the user that has been assigned as contributor.
        /// </summary>
        [Required]
        public string ContributorId { get; set; }
    }
}
