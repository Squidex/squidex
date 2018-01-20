// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.Api.Controllers.Users.Models
{
    public sealed class UsersDto
    {
        /// <summary>
        /// The total number of users.
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// The users.
        /// </summary>
        public UserDto[] Items { get; set; }
    }
}
