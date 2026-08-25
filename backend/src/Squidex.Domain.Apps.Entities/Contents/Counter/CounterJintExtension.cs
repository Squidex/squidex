// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Jint.Native;
using Jint.Runtime;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Properties;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents.Counter;

public sealed class CounterJintExtension(ICounterService counterService) : IJintExtension, IScriptDescriptor
{
    private delegate long CounterResetDelegate(string name, long value = 0);
    private delegate void CounterResetV2Delegate(string name, Action<JsValue>? callback = null, long value = 0);

    public void Extend(Engine engine)
    {
        var increment = new Func<string, long>(name =>
        {
            if (!engine.TryGetVar<DomainId>("appId", out var appId))
            {
                return 0;
            }

            return Increment(appId, name);
        });

        engine.SetValue("incrementCounter", increment);

        var reset = new CounterResetDelegate((name, value) =>
        {
            if (!engine.TryGetVar<DomainId>("appId", out var appId))
            {
                return 0;
            }

            return Reset(appId, name, value);
        });

        engine.SetValue("resetCounter", reset);
    }

    public void ExtendAsync(Engine engine)
    {
        var increment = new Action<string, Action<JsValue>>((name, callback) =>
        {
            if (!engine.TryGetVar<DomainId>("appId", out var appId))
            {
                throw new JavaScriptException("'incrementCounterV2' is not available in this script.");
            }

            IncrementV2(engine, appId, name, callback);
        });

        engine.SetValue("incrementCounterV2", increment);

        var reset = new CounterResetV2Delegate((name, callback, value) =>
        {
            if (!engine.TryGetVar<DomainId>("appId", out var appId))
            {
                throw new JavaScriptException("'resetCounterV2' is not available in this script.");
            }

            ResetV2(engine, appId, name, callback, value);
        });

        engine.SetValue("resetCounterV2", reset);
    }

    private long Increment(DomainId appId, string name)
    {
        return AsyncHelper.Sync(() => counterService.IncrementAsync(appId, name));
    }

    private void IncrementV2(Engine engine, DomainId appId, string name, Action<JsValue> callback)
    {
        engine.Schedule(ct => counterService.IncrementAsync(appId, name, ct),
            result => callback?.Invoke(JsValue.FromObject(engine, result)));
    }

    private long Reset(DomainId appId, string name, long value)
    {
        return AsyncHelper.Sync(() => counterService.ResetAsync(appId, name, value));
    }

    private void ResetV2(Engine engine, DomainId appId, string name, Action<JsValue>? callback, long value)
    {
        engine.Schedule(ct => counterService.ResetAsync(appId, name, value, ct),
            result => callback?.Invoke(JsValue.FromObject(engine, result)));
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
}
