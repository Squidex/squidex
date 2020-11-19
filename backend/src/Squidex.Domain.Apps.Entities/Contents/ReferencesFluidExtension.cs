// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Templates;
using Squidex.Infrastructure;

#pragma warning disable CA1826 // Do not use Enumerable methods on indexable collections

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ReferencesFluidExtension : IFluidExtension
    {
        private readonly IContentQueryService contentQueryService;
        private readonly IAppProvider appProvider;

        private sealed class ReferenceTag : ArgumentsTag
        {
            private readonly IContentQueryService contentQueryService;
            private readonly IAppProvider appProvider;

            public ReferenceTag(IContentQueryService contentQueryService, IAppProvider appProvider)
            {
                this.contentQueryService = contentQueryService;

                this.appProvider = appProvider;
            }

            public override async ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, FilterArgument[] arguments)
            {
                if (arguments.Length == 2 && context.GetValue("event")?.ToObjectValue() is EnrichedEvent enrichedEvent)
                {
                    var app = await appProvider.GetAppAsync(enrichedEvent.AppId.Id, false);

                    if (app == null)
                    {
                        return Completion.Normal;
                    }

                    var appContext =
                        Context.Admin()
                            .WithoutContentEnrichment()
                            .WithoutCleanup()
                            .WithUnpublished();

                    appContext.App = app;

                    var id = (await arguments[1].Expression.EvaluateAsync(context)).ToStringValue();

                    var domainId = DomainId.Create(id);
                    var domainIds = new List<DomainId> { domainId };

                    var references = await contentQueryService.QueryAsync(appContext, domainIds);
                    var reference = references.FirstOrDefault();

                    if (reference != null)
                    {
                        var name = (await arguments[0].Expression.EvaluateAsync(context)).ToStringValue();

                        context.SetValue(name, reference);
                    }
                }

                return Completion.Normal;
            }
        }

        public ReferencesFluidExtension(IContentQueryService contentQueryService, IAppProvider appProvider)
        {
            Guard.NotNull(contentQueryService, nameof(contentQueryService));
            Guard.NotNull(appProvider, nameof(appProvider));

            this.contentQueryService = contentQueryService;

            this.appProvider = appProvider;
        }

        public void RegisterGlobalTypes(IMemberAccessStrategy memberAccessStrategy)
        {
            memberAccessStrategy.Register<IContentEntity>();
        }

        public void RegisterLanguageExtensions(FluidParserFactory factory)
        {
            factory.RegisterTag("reference", new ReferenceTag(contentQueryService, appProvider));
        }
    }
}
