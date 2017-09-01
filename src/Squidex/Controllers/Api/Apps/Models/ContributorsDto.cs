// ==========================================================================
//  ContributorsDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Controllers.Api.Apps.Models
{
    public sealed class ContributorsDto
    {
        /// <summary>
        /// The contributors.
        /// </summary>
        [Required]
        public ContributorDto[] Contributors { get; set; }

        /// <summary>
        /// The maximum number of allowed contributors.
        /// </summary>
        public int MaxContributors { get; set; }
    }
}
