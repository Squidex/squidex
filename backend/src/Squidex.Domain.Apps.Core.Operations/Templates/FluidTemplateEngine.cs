// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Templates
{
    public sealed class FluidTemplateEngine : ITemplateEngine
    {
        private readonly IEnumerable<IFluidExtension> extensions;

        private sealed class SquidexTemplate : BaseFluidTemplate<SquidexTemplate>
        {
            public static void Setup(IEnumerable<IFluidExtension> extensions)
            {
                foreach (var extension in extensions)
                {
                    extension.RegisterLanguageExtensions(Factory);
                }
            }

            public static void SetupTypes(IEnumerable<IFluidExtension> extensions)
            {
                var globalTypes = TemplateContext.GlobalMemberAccessStrategy;

                globalTypes.MemberNameStrategy = MemberNameStrategies.CamelCase;

                foreach (var extension in extensions)
                {
                    extension.RegisterGlobalTypes(globalTypes);
                }

                foreach (var type in SquidexCoreModel.Assembly.GetTypes().Where(x => x.IsEnum))
                {
                    FluidValue.SetTypeMapping(type, x => new StringValue(x.ToString()));
                }

                FluidValue.SetTypeMapping<RefTokenType>(x => new StringValue(x.ToString().ToLowerInvariant()));

                globalTypes.Register<NamedId<DomainId>>();
                globalTypes.Register<NamedId<Guid>>();
                globalTypes.Register<NamedId<string>>();
                globalTypes.Register<NamedId<long>>();
                globalTypes.Register<RefToken>();
            }
        }

        public FluidTemplateEngine(IEnumerable<IFluidExtension> extensions)
        {
            this.extensions = extensions;

            SquidexTemplate.Setup(extensions);
            SquidexTemplate.SetupTypes(extensions);
        }

        public async Task<string> RenderAsync(string template, TemplateVars variables)
        {
            Guard.NotNull(variables, nameof(variables));

            if (SquidexTemplate.TryParse(template, out var parsed, out var errors))
            {
                var context = new TemplateContext();

                foreach (var extension in extensions)
                {
                    extension.BeforeRun(context);
                }

                foreach (var (key, value) in variables)
                {
                    context.MemberAccessStrategy.Register(value.GetType());

                    context.SetValue(key, value);
                }

                var result = await parsed.RenderAsync(context);

                return result;
            }

            throw new TemplateParseException(template, errors);
        }
    }
}
