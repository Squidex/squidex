// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint.Native;
using Orleans;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Properties;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents.Counter
{
    public sealed class CounterJintExtension : IJintExtension, IScriptDescriptor
    {
        private delegate long CounterReset(string name, long value = 0);
        private delegate void CounterResetV2(string name, Action<JsValue>? callback = null, long value = 0);
        private readonly IGrainFactory grainFactory;

        public CounterJintExtension(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public void Extend(ScriptExecutionContext context)
        {
            if (!context.TryGetValue<DomainId>("appId", out var appId))
            {
                return;
            }

            var increment = new Func<string, long>(name =>
            {
                return Increment(appId, name);
            });

            context.Engine.SetValue("incrementCounter", increment);

            var reset = new CounterReset((name, value) =>
            {
                return Reset(appId, name, value);
            });

            context.Engine.SetValue("resetCounter", reset);
        }

        public void ExtendAsync(ScriptExecutionContext context)
        {
            if (!context.TryGetValue<DomainId>("appId", out var appId))
            {
                return;
            }

            var increment = new Action<string, Action<JsValue>>((name, callback) =>
            {
                IncrementV2(context, appId, name, callback);
            });

            context.Engine.SetValue("incrementCounterV2", increment);

            var reset = new CounterResetV2((name, callback, value) =>
            {
                ResetV2(context, appId, name, callback, value);
            });

            context.Engine.SetValue("resetCounterV2", reset);
        }

        public void Describe(AddDescription describe, ScriptScope scope)
        {
            describe(JsonType.Function, "incrementCounter(name)",
                Resources.ScriptingIncrementCounter);

            describe(JsonType.Function, "incrementCounterV2(name, callback?)",
                Resources.ScriptingIncrementCounterV2);

            describe(JsonType.Function, "resetCounter(name, value?)",
                Resources.ScriptingResetCounter);

            describe(JsonType.Function, "resetCounter(name, callback?, value?)",
                Resources.ScriptingResetCounterV2);
        }

        private long Increment(DomainId appId, string name)
        {
            var grain = grainFactory.GetGrain<ICounterGrain>(appId.ToString());

            return AsyncHelper.Sync(() => grain.IncrementAsync(name));
        }

        private void IncrementV2(ScriptExecutionContext context, DomainId appId, string name, Action<JsValue> callback)
        {
            context.Schedule(async (scheduler, ct) =>
            {
                var grain = grainFactory.GetGrain<ICounterGrain>(appId.ToString());

                var result = await grain.IncrementAsync(name);

                if (callback != null)
                {
                    scheduler.Run(callback, JsValue.FromObject(context.Engine, result));
                }
            });
        }

        private long Reset(DomainId appId, string name, long value)
        {
            var grain = grainFactory.GetGrain<ICounterGrain>(appId.ToString());

            return AsyncHelper.Sync(() => grain.ResetAsync(name, value));
        }

        private void ResetV2(ScriptExecutionContext context, DomainId appId, string name, Action<JsValue>? callback, long value)
        {
            context.Schedule(async (scheduler, ct) =>
            {
                var grain = grainFactory.GetGrain<ICounterGrain>(appId.ToString());

                var result = await grain.ResetAsync(name, value);

                if (callback != null)
                {
                    scheduler.Run(callback, JsValue.FromObject(context.Engine, result));
                }
            });
        }
    }
}
