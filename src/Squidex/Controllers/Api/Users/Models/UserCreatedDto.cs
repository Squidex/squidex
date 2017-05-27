// ==========================================================================
//  UserCreatedDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Controllers.Api.Users.Models
{
    public class UserCreatedDto
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string PictureUrl { get; set; }
    }
}
