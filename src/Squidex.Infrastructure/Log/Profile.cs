// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Squidex.Infrastructure.Log
{
    public static class Profile
    {
        private static ILogProfilerSessionProvider sessionProvider;

        private sealed class Timer : IDisposable
        {
            private readonly Stopwatch watch = Stopwatch.StartNew();
            private readonly ProfilerSession session;
            private readonly string key;

            public Timer(ProfilerSession session, string key)
            {
                this.session = session;
                this.key = key;
            }

            public void Dispose()
            {
                watch.Stop();

                session.Measured(key, watch.ElapsedMilliseconds);
            }
        }

        public static void Init(ILogProfilerSessionProvider provider)
        {
            sessionProvider = provider;
        }

        public static IDisposable Method<T>([CallerMemberName] string memberName = null)
        {
            return Key($"{typeof(T).Name}/{memberName}");
        }

        public static IDisposable Method(string objectName, [CallerMemberName] string memberName = null)
        {
            return Key($"{objectName}/{memberName}");
        }

        public static IDisposable Key(string key)
        {
            Guard.NotNull(key, nameof(key));

            var session = sessionProvider?.GetSession();

            if (session == null)
            {
                return NoopDisposable.Instance;
            }

            return new Timer(session, key);
        }
    }
}
