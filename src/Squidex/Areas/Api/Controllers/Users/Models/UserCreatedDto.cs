// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Areas.Api.Controllers.Users.Models
{
    public sealed class UserCreatedDto
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string PictureUrl { get; set; }
    }
}
