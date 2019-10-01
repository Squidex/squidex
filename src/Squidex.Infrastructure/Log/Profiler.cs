// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Log
{
    public delegate void ProfilerStarted(ProfilerSpan span);

    public static class Profiler
    {
        private static readonly AsyncLocal<ProfilerSession> LocalSession = new AsyncLocal<ProfilerSession>();
        private static readonly AsyncLocalCleaner<ProfilerSession> Cleaner;

        public static ProfilerSession? Session
        {
            get { return LocalSession.Value; }
        }

        public static event ProfilerStarted SpanStarted;

        static Profiler()
        {
            Cleaner = new AsyncLocalCleaner<ProfilerSession>(LocalSession);
        }

        public static IDisposable StartSession()
        {
            LocalSession.Value = new ProfilerSession();

            return Cleaner;
        }

        public static IDisposable TraceMethod(Type type, [CallerMemberName] string? memberName = null)
        {
            return Trace($"{type.Name}/{memberName}");
        }

        public static IDisposable TraceMethod<T>([CallerMemberName] string? memberName = null)
        {
            return Trace($"{typeof(T).Name}/{memberName}");
        }

        public static IDisposable TraceMethod(string objectName, [CallerMemberName] string? memberName = null)
        {
            return Trace($"{objectName}/{memberName}");
        }

        public static IDisposable Trace(string key)
        {
            Guard.NotNull(key, nameof(key));

            var session = LocalSession.Value;

            if (session == null)
            {
                return NoopDisposable.Instance;
            }

            var span = new ProfilerSpan(session, key);

            SpanStarted?.Invoke(span);

            return span;
        }
    }
}
