// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Http;

namespace Squidex.Web.Pipeline
{
    public interface IExceptionHandler
    {
        void Handle(Exception exception, HttpContext? httpContext = null);
    }
}
