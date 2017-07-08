// ==========================================================================
//  UsersDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
namespace Squidex.Controllers.Api.Users.Models
{
    public class UsersDto
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
