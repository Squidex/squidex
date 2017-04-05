// ==========================================================================
//  ApplicationInfoLogAppender.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Reflection;

namespace Squidex.Infrastructure.Log
{
    public sealed class ApplicationInfoLogAppender : ILogAppender
    {
        private readonly string applicationName;
        private readonly string applicationVersion;
        private readonly string applicationSessionId;

        public ApplicationInfoLogAppender(Assembly assembly)
        {
            Guard.NotNull(assembly, nameof(assembly));

            applicationName = assembly.GetName().Name;
            applicationVersion = assembly.GetName().Version.ToString();
            applicationSessionId = Guid.NewGuid().ToString();
        }

        public void Append(IObjectWriter writer)
        {
            writer.WriteProperty("appName", applicationName);
            writer.WriteProperty("appVersion", applicationVersion);
            writer.WriteProperty("appSessionId", applicationSessionId);
        }
    }
}
