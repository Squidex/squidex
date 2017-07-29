// ==========================================================================
//  UpdateAppClientDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Controllers.Api.Apps.Models
{
    public class UpdateAppClientDto
    {
        /// <summary>
        /// The new display name of the client.
        /// </summary>
        [StringLength(20)]
        public string Name { get; set; }

        /// <summary>
        /// Determines if the client is a reader.
        /// </summary>
        public bool? IsReader { get; set; }
    }
}
