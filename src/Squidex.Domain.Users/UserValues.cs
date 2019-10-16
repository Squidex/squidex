﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Squidex.Infrastructure.Security;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Users
{
    public sealed class UserValues
    {
        public string DisplayName { get; set; }

        public string PictureUrl { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public bool? Invited { get; set; }

        public bool? Consent { get; set; }

        public bool? ConsentForEmails { get; set; }

        public bool? Hidden { get; set; }

        public PermissionSet Permissions { get; set; }

        public List<Claim> ToClaims(bool initial)
        {
            return ToClaimsCore(initial).ToList();
        }

        private IEnumerable<Claim> ToClaimsCore(bool initial)
        {
            if (!string.IsNullOrWhiteSpace(DisplayName))
            {
                yield return new Claim(SquidexClaimTypes.DisplayName, DisplayName);
            }

            if (!string.IsNullOrWhiteSpace(PictureUrl))
            {
                yield return new Claim(SquidexClaimTypes.PictureUrl, PictureUrl);
            }

            if (Hidden.HasValue)
            {
                yield return new Claim(SquidexClaimTypes.Hidden, Hidden.ToString());
            }

            if (Invited.HasValue)
            {
                yield return new Claim(SquidexClaimTypes.Invited, Invited.ToString());
            }

            if (Consent.HasValue)
            {
                yield return new Claim(SquidexClaimTypes.Consent, Consent.ToString());
            }

            if (ConsentForEmails.HasValue)
            {
                yield return new Claim(SquidexClaimTypes.ConsentForEmails, ConsentForEmails.ToString());
            }

            if (Permissions != null)
            {
                if (!initial)
                {
                    yield return new Claim(SquidexClaimTypes.Permissions, string.Empty);
                }

                foreach (var permission in Permissions)
                {
                    yield return new Claim(SquidexClaimTypes.Permissions, permission.Id);
                }
            }
        }
    }
}
