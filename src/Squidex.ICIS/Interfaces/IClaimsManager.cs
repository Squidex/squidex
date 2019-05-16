// ==========================================================================
//  ICIS Copyright
// ==========================================================================
//  Copyright (c) ICIS
//  All rights reserved.
// ==========================================================================

using System.Security.Claims;
using Squidex.ICIS.Models;

namespace Squidex.ICIS.Interfaces
{
    public interface IClaimsManager
    {
        UserInfo CreateUserWithClaims(ClaimsIdentity identity);
    }
}