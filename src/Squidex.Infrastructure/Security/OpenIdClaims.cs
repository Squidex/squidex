// ==========================================================================
//  OpenIdClaims.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.Security
{
    public class OpenIdClaims
    {
        /// <summary>
        /// Unique Identifier for the End-User at the Issuer.
        /// </summary>
        public const string Subject = "sub";

        /// <summary>
        /// End-User's full name in displayable form including all name parts, possibly including titles and suffixes, ordered according to the End-User's locale and preferences.
        /// </summary>
        public const string Name = "name";

        /// <summary>
        /// Casual name of the End-User that may or may not be the same as the given_name. For instance, a nickname value of Mike might be returned alongside a given_name value of Michael.
        /// </summary>
        public const string NickName = "nickname";

        /// <summary>
        /// Shorthand name by which the End-User wishes to be referred to at the
        /// </summary>
        public const string PreferredUserName = "preferred_username";

        /// <summary>
        /// End-User's preferred e-mail address.
        /// </summary>
        public const string Email = "email";
    }
}
