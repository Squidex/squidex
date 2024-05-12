// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;

namespace Squidex.Areas.IdentityServer.Config;

public sealed class DynamicOpenIdConnectHandler : OpenIdConnectHandler
{
    public DynamicOpenIdConnectHandler(IOptionsMonitor<DynamicOpenIdConnectOptions> options, ILoggerFactory logger, HtmlEncoder htmlEncoder, UrlEncoder encoder)
        : base(options, logger, htmlEncoder, encoder)
    {
    }
}
