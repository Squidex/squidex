// ==========================================================================
//  RenameClientDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Controllers.Api.Apps.Models
{
    public class RenameClientDto
    {
        /// <summary>
        /// The new display name of the client.
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Name { get; set; }
    }
}
