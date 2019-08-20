// ==========================================================================
//  ICIS Copyright
// ==========================================================================
//  Copyright (c) ICIS
//  All rights reserved.
// ==========================================================================

using System.Security.Claims;

namespace Squidex.ICIS.Authentication.User
{
    public interface IUserManager
    {
        UserInfo GetUserInfo(ClaimsIdentity identity);
    }
}