// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Security;

public static class OpenIdClaims
{
    /// <summary>
    /// Unique Identifier for the End-User at the Issuer.
    /// </summary>
    public static readonly string Subject = "sub";

    /// <summary>
    /// The client id claim.
    /// </summary>
    public static readonly string ClientId = "client_id";

    /// <summary>
    /// End-User's full name in displayable form including all name parts, possibly including titles and suffixes, ordered according to the End-User's locale and preferences.
    /// </summary>
    public static readonly string Name = "name";

    /// <summary>
    /// Casual name of the End-User that may or may not be the same as the given_name. For instance, a nickname value of Mike might be returned alongside a given_name value of Michael.
    /// </summary>
    public static readonly string NickName = "nickname";

    /// <summary>
    /// Shorthand name by which the End-User wishes to be referred to.
    /// </summary>
    public static readonly string PreferredUserName = "preferred_username";

    /// <summary>
    /// End-User's preferred e-mail address.
    /// </summary>
    public static readonly string Email = "email";
}
