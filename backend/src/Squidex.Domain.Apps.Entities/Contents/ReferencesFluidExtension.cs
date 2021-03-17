// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using GraphQL.Utilities;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Templates;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;

#pragma warning disable CA1826 // Do not use Enumerable methods on indexable collections

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ReferencesFluidExtension : IFluidExtension
    {
        private readonly IServiceProvider serviceProvider;

        private sealed class ReferenceTag : ArgumentsTag
        {
            private readonly IServiceProvider serviceProvider;

            public ReferenceTag(IServiceProvider serviceProvider)
            {
                this.serviceProvider = serviceProvider;
            }

            public override async ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, FilterArgument[] arguments)
            {
                if (arguments.Length == 2 && context.GetValue("event")?.ToObjectValue() is EnrichedEvent enrichedEvent)
                {
                    var app = await GetAppAsync(enrichedEvent);

                    if (app == null)
                    {
                        return Completion.Normal;
                    }

                    var requestContext =
                        Context.Admin(app).Clone(b => b
                            .WithoutContentEnrichment()
                            .WithUnpublished()
                            .WithoutTotal());

                    var id = (await arguments[1].Expression.EvaluateAsync(context)).ToStringValue();

                    var domainId = DomainId.Create(id);
                    var domainIds = new List<DomainId> { domainId };

                    var contentQuery = serviceProvider.GetRequiredService<IContentQueryService>();

                    var contents = await contentQuery.QueryAsync(requestContext, Q.Empty.WithIds(domainIds));
                    var content = contents.FirstOrDefault();

                    if (content != null)
                    {
                        var name = (await arguments[0].Expression.EvaluateAsync(context)).ToStringValue();

                        context.SetValue(name, content);
                    }
                }

                return Completion.Normal;
            }

            private Task<IAppEntity?> GetAppAsync(EnrichedEvent enrichedEvent)
            {
                var appProvider = serviceProvider.GetRequiredService<IAppProvider>();

                return appProvider.GetAppAsync(enrichedEvent.AppId.Id, false);
            }
        }

        public ReferencesFluidExtension(IServiceProvider serviceProvider)
        {
            Guard.NotNull(serviceProvider, nameof(serviceProvider));

            this.serviceProvider = serviceProvider;
        }

        public void RegisterGlobalTypes(IMemberAccessStrategy memberAccessStrategy)
        {
            memberAccessStrategy.Register<IContentEntity>();
            memberAccessStrategy.Register<IEntity>();
            memberAccessStrategy.Register<IEntityWithCreatedBy>();
            memberAccessStrategy.Register<IEntityWithLastModifiedBy>();
            memberAccessStrategy.Register<IEntityWithVersion>();
            memberAccessStrategy.Register<IEnrichedContentEntity>();
        }

        public void RegisterLanguageExtensions(FluidParserFactory factory)
        {
            factory.RegisterTag("reference", new ReferenceTag(serviceProvider));
        }
    }
}
