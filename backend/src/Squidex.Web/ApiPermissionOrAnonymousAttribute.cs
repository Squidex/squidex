// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authorization;

namespace Squidex.Web;

public class ApiPermissionOrAnonymousAttribute(params string[] ids) : ApiPermissionAttribute(ids), IAllowAnonymous
{
}
