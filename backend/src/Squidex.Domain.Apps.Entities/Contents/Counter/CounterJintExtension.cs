﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint.Native;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Properties;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents.Counter;

public sealed class CounterJintExtension(ICounterService counterService) : IJintExtension, IScriptDescriptor
{
    private delegate long CounterResetDelegate(string name, long value = 0);
    private delegate void CounterResetV2Delegate(string name, Action<JsValue>? callback = null, long value = 0);

    public void Extend(ScriptExecutionContext context)
    {
        if (!context.TryGetValueIfExists<DomainId>("appId", out var appId))
        {
            return;
        }

        var increment = new Func<string, long>(name =>
        {
            return Increment(appId, name);
        });

        context.Engine.SetValue("incrementCounter", increment);

        var reset = new CounterResetDelegate((name, value) =>
        {
            return Reset(appId, name, value);
        });

        context.Engine.SetValue("resetCounter", reset);
    }

    public void ExtendAsync(ScriptExecutionContext context)
    {
        if (!context.TryGetValueIfExists<DomainId>("appId", out var appId))
        {
            return;
        }

        var increment = new Action<string, Action<JsValue>>((name, callback) =>
        {
            IncrementV2(context, appId, name, callback);
        });

        context.Engine.SetValue("incrementCounterV2", increment);

        var reset = new CounterResetV2Delegate((name, callback, value) =>
        {
            ResetV2(context, appId, name, callback, value);
        });

        context.Engine.SetValue("resetCounterV2", reset);
    }

    private long Increment(DomainId appId, string name)
    {
        return AsyncHelper.Sync(() => counterService.IncrementAsync(appId, name));
    }

    private void IncrementV2(ScriptExecutionContext context, DomainId appId, string name, Action<JsValue> callback)
    {
        context.Schedule(async (scheduler, ct) =>
        {
            var result = await counterService.IncrementAsync(appId, name, ct);

            if (callback != null)
            {
                scheduler.Run(callback, JsValue.FromObject(context.Engine, result));
            }
        });
    }

    private long Reset(DomainId appId, string name, long value)
    {
        return AsyncHelper.Sync(() => counterService.ResetAsync(appId, name, value));
    }

    private void ResetV2(ScriptExecutionContext context, DomainId appId, string name, Action<JsValue>? callback, long value)
    {
        context.Schedule(async (scheduler, ct) =>
        {
            var result = await counterService.ResetAsync(appId, name, value, ct);

            if (callback != null)
            {
                scheduler.Run(callback, JsValue.FromObject(context.Engine, result));
            }
        });
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
