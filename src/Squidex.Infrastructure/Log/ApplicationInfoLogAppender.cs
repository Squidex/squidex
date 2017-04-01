// ==========================================================================
//  ApplicationInfoLogAppender.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Reflection;

namespace Squidex.Infrastructure.Log
{
    public sealed class ApplicationInfoLogAppender : ILogAppender
    {
        private readonly string applicationName;
        private readonly string applicationVersion;

        public ApplicationInfoLogAppender(Assembly assembly)
        {
            Guard.NotNull(assembly, nameof(assembly));

            applicationName = assembly.GetName().Name;
            applicationVersion = assembly.GetName().Version.ToString();
        }

        public void Append(IObjectWriter writer)
        {
            writer.WriteProperty("applicationName", applicationName);
            writer.WriteProperty("applicationVersion", applicationVersion);
        }
    }
}
