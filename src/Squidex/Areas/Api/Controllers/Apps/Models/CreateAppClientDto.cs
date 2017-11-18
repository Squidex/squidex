// ==========================================================================
//  CreateAppClientDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class CreateAppClientDto
    {
        /// <summary>
        /// The id of the client.
        /// </summary>
        [Required]
        [RegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
        public string Id { get; set; }
    }
}
