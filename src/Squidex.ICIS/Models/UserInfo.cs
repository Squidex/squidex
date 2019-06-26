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
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Users;
using Squidex.Infrastructure.Security;

namespace Squidex.ICIS.Models
{
    public class UserInfo
    {
        private const string GroupType = "http://schemas.xmlsoap.org/claims/Group";
        private readonly ClaimsIdentity identity;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppProvider _appProvider;
        private readonly IConfiguration _config;

        public UserInfo(ClaimsIdentity identity, UserManager<IdentityUser> userManager, AppProvider appProvider, IConfiguration config)
        {
            _userManager = userManager;
            _appProvider = appProvider;
            _config = config;
            this.identity = identity;
            Email = GetValue(OpenIdClaims.Email);
            GivenName = GetValue(ClaimTypes.GivenName);
            Group = GetValues(GroupType);
            Permissions = SetPermissions(Email);
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

        private PermissionSet SetPermissions(string email)
        {
            // Get configuration values
            var adminUserGroupConfig = _config.GetSection("admin:adminUserGroup").GetChildren().ToArray();
            var adminUserGroupConfigArray = adminUserGroupConfig.Select(item => item.Value.ToString()).ToArray();
            var appsConfig = _config.GetSection("admin:adminApps").GetChildren().AsEnumerable();

            // If the user has a user group that matches an administrator user group
            if (adminUserGroupConfigArray.Intersect(Group.ToArray()).Any()) 
            {
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
            else
            {
                var user = UserManagerExtensions.QueryUsers(_userManager, email).ToList().FirstOrDefault();
                var permissionsList = new List<string>();
                //Check if user exists in DB
                if (user != null)
                {
                    //Query all apps for this user
                    var apps = _appProvider.GetUserApps(user.Id, new PermissionSet(new List<string>())).Result;

                    //For each app, get the user's role, and get the permissions
                    foreach (var appEntity in apps)
                    {
                        var role = appEntity.Contributors[user.Id];
                        var permissions = appEntity.Roles[role].Permissions;
                        foreach (var permission in permissions)
                        {
                            permissionsList.Add(permission.Id);
                        }
                    }

                    return new PermissionSet(permissionsList);
                }

                //If No:
                //Assign Empty Permissions
                return new PermissionSet(new List<string>());
            }

        }
    }
}