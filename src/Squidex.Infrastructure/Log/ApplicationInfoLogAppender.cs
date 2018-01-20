// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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

        public ApplicationInfoLogAppender(Type type, Guid applicationSession)
            : this(type?.GetTypeInfo().Assembly, applicationSession)
        {
        }

        public ApplicationInfoLogAppender(Assembly assembly, Guid applicationSession)
        {
            Guard.NotNull(assembly, nameof(assembly));

            applicationName = assembly.GetName().Name;
            applicationVersion = assembly.GetName().Version.ToString();
            applicationSessionId = applicationSession.ToString();
        }

        public void Append(IObjectWriter writer)
        {
            writer.WriteObject("app", w => w
                .WriteProperty("name", applicationName)
                .WriteProperty("version", applicationVersion)
                .WriteProperty("sessionId", applicationSessionId));
        }
    }
}
