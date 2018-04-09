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
        /// <summary>
        /// The id of the user.
        /// </summary>
        [Required]
        public string Id { get; set; }
    }
}
