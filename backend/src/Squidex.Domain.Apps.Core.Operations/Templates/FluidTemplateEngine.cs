// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;
using Fluid.Values;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Templates;

public sealed class FluidTemplateEngine : ITemplateEngine
{
    private readonly TemplateOptions options = new TemplateOptions();
    private readonly CustomFluidParser parser = new CustomFluidParser();

    public FluidTemplateEngine(IEnumerable<IFluidExtension> extensions)
    {
        options.MemberAccessStrategy = new UnsafeMemberAccessStrategy
        {
            MemberNameStrategy = MemberNameStrategies.CamelCase
        };

        foreach (var extension in extensions)
        {
            extension.RegisterLanguageExtensions(parser, options);
        }

        options.ValueConverters.Add(value =>
        {
            if (value is RefTokenType tokenType)
            {
                return StringValue.Create(tokenType.ToString().ToLowerInvariant());
            }

            if (value?.GetType().IsEnum == true)
            {
                return new StringValue(value.ToString());
            }

            return null;
        });
    }

    public async Task<string> RenderAsync(string template, TemplateVars variables)
    {
        Guard.NotNull(variables);

        if (!parser.TryParse(template, out var parsed, out var error))
        {
            throw new TemplateParseException(template, error);
        }

        var context = new TemplateContext(options);

        foreach (var (key, value) in variables)
        {
            context.SetValue(key, value);
        }

        var result = await parsed.RenderAsync(context);

        return result;
    }
}
