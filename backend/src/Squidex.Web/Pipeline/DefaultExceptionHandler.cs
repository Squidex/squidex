// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;

namespace Squidex.Web.Pipeline
{
    public class DefaultExceptionHandler : IExceptionHandler
    {
        private readonly ISemanticLog log;

        public DefaultExceptionHandler(ISemanticLog log)
        {
            Guard.NotNull(log);

            this.log = log;
        }

        public void Handle(Exception ex, HttpContext? httpContext = null)
        {
            log.LogError(ex, w => w.WriteProperty("status", "UnhandledException"));
        }
    }
}
