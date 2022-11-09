// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Text.Encodings.Web;
using Fluid;
using Fluid.Values;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Templates.Extensions;

public sealed class JsonArrayFluidValue : FluidValue
{
    private readonly JsonArray value;

    public override FluidValues Type { get; } = FluidValues.Array;

    public JsonArrayFluidValue(JsonArray value)
    {
        this.value = value;
    }

    public override bool Equals(FluidValue other)
    {
        return other is JsonArrayFluidValue array && array.value.Equals(value);
    }

    public override bool ToBooleanValue()
    {
        return true;
    }

    public override decimal ToNumberValue()
    {
        return 0;
    }

    public override object ToObjectValue()
    {
        return new ObjectValue(value);
    }

    public override string ToStringValue()
    {
        return value.ToString()!;
    }

    protected override FluidValue GetValue(string name, TemplateContext context)
    {
        switch (name)
        {
            case "size":
                return NumberValue.Create(value.Count);

            case "first":
                if (value.Count > 0)
                {
                    return Create(value[0]);
                }

                break;

            case "last":
                if (value.Count > 0)
                {
                    return Create(value[^1]);
                }

                break;
        }

        return NilValue.Instance;
    }

    protected override FluidValue GetIndex(FluidValue index, TemplateContext context)
    {
        var i = (int)index.ToNumberValue();

        if (i >= 0 && i < value.Count)
        {
            return Create(value[i]);
        }

        return NilValue.Instance;
    }

    public override IEnumerable<FluidValue> Enumerate()
    {
        foreach (var item in value)
        {
            yield return Create(item);
        }
    }

    public override void WriteTo(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo)
    {
        writer.Write(value);
    }
}
