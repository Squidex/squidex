// =========================================================================
//  UserDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Modules.Api.Users.Models
{
    public sealed class UserDto
    {
        public string Id { get; set; }

        public string ProfileUrl { get; set; }

        public string DisplayName { get; set; }
    }
}
