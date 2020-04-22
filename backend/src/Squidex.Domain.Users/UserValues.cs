// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Squidex.Infrastructure.Security;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Users
{
    public sealed class UserValues
    {
        public string? DisplayName { get; set; }

        public string? PictureUrl { get; set; }

        public string? Password { get; set; }

        public string? ClientSecret { get; set; }

        public string Email { get; set; }

        public bool? Hidden { get; set; }

        public bool? Invited { get; set; }

        public bool? Consent { get; set; }

        public bool? ConsentForEmails { get; set; }

        public PermissionSet? Permissions { get; set; }

        public List<Claim>? CustomClaims { get; set; }

        public List<(string Name, string Value)>? Properties { get; set; }

        internal async Task<IdentityResult> SyncClaims(UserManager<IdentityUser> userManager, IdentityUser user)
        {
            var current = await userManager.GetClaimsAsync(user);

            var claimsToRemove = new List<Claim>();
            var claimsToAdd = new List<Claim>();

            void RemoveClaims(Func<Claim, bool> predicate)
            {
                claimsToAdd.RemoveAll(x => predicate(x));
                claimsToRemove.AddRange(current.Where(predicate));
            }

            void AddClaim(string type, string value)
            {
                claimsToAdd.Add(new Claim(type, value));
            }

            void SyncString(string type, string? value)
            {
                if (value != null)
                {
                    RemoveClaims(x => x.Type == type);

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        AddClaim(type, value);
                    }
                }
            }

            void SyncBoolean(string type, bool? value)
            {
                if (value != null)
                {
                    RemoveClaims(x => x.Type == type);

                    if (value == true)
                    {
                        AddClaim(type, value.ToString()!);
                    }
                }
            }

            SyncString(SquidexClaimTypes.ClientSecret, ClientSecret);
            SyncString(SquidexClaimTypes.DisplayName, DisplayName);
            SyncString(SquidexClaimTypes.PictureUrl, PictureUrl);

            SyncBoolean(SquidexClaimTypes.Hidden, Hidden);
            SyncBoolean(SquidexClaimTypes.Invited, Invited);
            SyncBoolean(SquidexClaimTypes.Consent, Consent);
            SyncBoolean(SquidexClaimTypes.ConsentForEmails, ConsentForEmails);

            if (Permissions != null)
            {
                RemoveClaims(x => x.Type == SquidexClaimTypes.Permissions);

                foreach (var permission in Permissions)
                {
                    AddClaim(SquidexClaimTypes.Permissions, permission.Id);
                }
            }

            if (Properties != null)
            {
                RemoveClaims(x => x.Type.StartsWith(SquidexClaimTypes.CustomPrefix, StringComparison.OrdinalIgnoreCase));

                foreach (var (name, value) in Properties)
                {
                    AddClaim($"{SquidexClaimTypes.CustomPrefix}:{name}", value);
                }
            }

            if (CustomClaims != null)
            {
                foreach (var group in CustomClaims.GroupBy(x => x.Type))
                {
                    RemoveClaims(x => x.Type == group.Key);

                    foreach (var claim in group)
                    {
                        AddClaim(claim.Type, claim.Value);
                    }
                }
            }

            if (claimsToRemove.Count > 0)
            {
                var result = await userManager.RemoveClaimsAsync(user, claimsToRemove);

                if (!result.Succeeded)
                {
                    return result;
                }
            }

            if (claimsToAdd.Count > 0)
            {
                return await userManager.AddClaimsAsync(user, claimsToAdd);
            }

            return IdentityResult.Success;
        }
    }
}
