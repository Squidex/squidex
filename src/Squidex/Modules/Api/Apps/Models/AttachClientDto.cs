// ==========================================================================
//  AttachClientDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Modules.Api.Apps.Models
{
    public class AttachClientDto
    {
        /// <summary>
        /// The name of the client.
        /// </summary>
        [Required]
        [RegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
        public string ClientName { get; set; }
    }
}
