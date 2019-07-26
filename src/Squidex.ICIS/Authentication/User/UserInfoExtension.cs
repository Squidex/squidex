// ==========================================================================
//  ICIS Copyright
// ==========================================================================
//  Copyright (c) ICIS
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Users;

namespace Squidex.ICIS.Authentication.User
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