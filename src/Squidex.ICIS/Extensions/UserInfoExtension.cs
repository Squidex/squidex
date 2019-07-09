// ==========================================================================
//  ICIS Copyright
// ==========================================================================
//  Copyright (c) ICIS
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Users;
using Squidex.ICIS.Models;

namespace Squidex.ICIS.Extensions
{
    public static class UserInfoExtension
    {

        public static UserValues ToUserValues(this UserInfo userInfo)
        {
            return new UserValues
            {
                Email = userInfo.Email,
                Permissions = userInfo.Permissions,
                DisplayName = userInfo.GivenName,
                Consent = true,
                ConsentForEmails = true,
            };
        }
    }
}