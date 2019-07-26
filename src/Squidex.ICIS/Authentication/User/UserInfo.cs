// ==========================================================================
//  ICIS Copyright
// ==========================================================================
//  Copyright (c) ICIS
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Squidex.Infrastructure.Security;

namespace Squidex.ICIS.Authentication.User
{
    public class UserInfo
    {
        private const string GroupType = "http://schemas.xmlsoap.org/claims/Group";
        private readonly ClaimsIdentity identity;
        private readonly IConfiguration _config;

        public UserInfo(ClaimsIdentity identity, IConfiguration config)
        {
            _config = config;
            this.identity = identity;
            Email = GetValue(OpenIdClaims.Email);
            GivenName = GetValue(ClaimTypes.GivenName);
            Group = GetValues(GroupType);
            Permissions = SetPermissions();
        }

        public string Email { get; }

        public string UserId { get; set; }

        public string GivenName { get; }

        public List<string> Group { get; }

        public PermissionSet Permissions { get; }

        private string GetValue(string key)
        {
            return identity.Claims.FirstOrDefault(claim => claim.Type.Equals(key, StringComparison.OrdinalIgnoreCase))
                ?.Value;
        }

        private List<string> GetValues(string key)
        {
            return identity.Claims.Where(claim => claim.Type.Equals(key, StringComparison.OrdinalIgnoreCase))
                .Select(claim => claim.Value).ToList();
        }

        private PermissionSet SetPermissions()
        {
            // Get configuration values
            var adminUserGroupConfig = _config.GetSection("admin:adminUserGroup").GetChildren().ToArray();
            var adminUserGroupConfigArray = adminUserGroupConfig.Select(item => item.Value.ToString()).ToArray();

            // If the user has a user group that matches an administrator user group
            if (adminUserGroupConfigArray.Intersect(Group.ToArray()).Any()) 
            {
                var appsConfig = _config.GetSection("admin:adminApps").GetChildren().AsEnumerable();

                // Add base permissions for administrator
                var adminPermissions = new List<string>()
                {
                    Shared.Permissions.All,
                    Shared.Permissions.Admin
                };

                // Get the list of apps to enable full app-permissions
                foreach (var app in appsConfig)
                {
                    adminPermissions.Add(
                        Shared.Permissions.App.Replace("{app}", app.Value)
                    );
                }

                return new PermissionSet(adminPermissions);
            }

            return new PermissionSet(new List<string>());
            

        }
    }
}