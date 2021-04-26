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
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Log;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Users
{
    internal static class UserManagerExtensions
    {
        public static async Task Throw(this Task<IdentityResult> task, ISemanticLog log)
        {
            var result = await task;

            static string Localize(IdentityError error)
            {
                if (!string.IsNullOrWhiteSpace(error.Code))
                {
                    return T.Get($"dotnet_identity_{error.Code}", error.Description);
                }
                else
                {
                    return error.Description;
                }
            }

            if (!result.Succeeded)
            {
                var errorMessageBuilder = new StringBuilder();

                foreach (var error in result.Errors)
                {
                    errorMessageBuilder.Append(error.Code);
                    errorMessageBuilder.Append(": ");
                    errorMessageBuilder.AppendLine(error.Description);
                }

                var errorMessage = errorMessageBuilder.ToString();

                log.LogError(errorMessage, (ctx, w) => w
                    .WriteProperty("action", "IdentityOperation")
                    .WriteProperty("status", "Failed")
                    .WriteProperty("message", ctx));

                throw new ValidationException(result.Errors.Select(x => new ValidationError(Localize(x))).ToList());
            }
        }

        public static async Task<IdentityResult> SyncClaims(this UserManager<IdentityUser> userManager, IdentityUser user, UserValues values)
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

            SyncString(SquidexClaimTypes.ClientSecret, values.ClientSecret);
            SyncString(SquidexClaimTypes.DisplayName, values.DisplayName);
            SyncString(SquidexClaimTypes.PictureUrl, values.PictureUrl);

            SyncBoolean(SquidexClaimTypes.Hidden, values.Hidden);
            SyncBoolean(SquidexClaimTypes.Invited, values.Invited);
            SyncBoolean(SquidexClaimTypes.Consent, values.Consent);
            SyncBoolean(SquidexClaimTypes.ConsentForEmails, values.ConsentForEmails);

            if (values.Permissions != null)
            {
                RemoveClaims(x => x.Type == SquidexClaimTypes.Permissions);

                foreach (var permission in values.Permissions)
                {
                    AddClaim(SquidexClaimTypes.Permissions, permission.Id);
                }
            }

            if (values.Properties != null)
            {
                RemoveClaims(x => x.Type.StartsWith(SquidexClaimTypes.CustomPrefix, StringComparison.OrdinalIgnoreCase));

                foreach (var (name, value) in values.Properties)
                {
                    AddClaim($"{SquidexClaimTypes.CustomPrefix}:{name}", value);
                }
            }

            if (values.CustomClaims != null)
            {
                foreach (var group in values.CustomClaims.GroupBy(x => x.Type))
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
