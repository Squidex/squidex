// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using GraphQL.Utilities;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Templates;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetsFluidExtension : IFluidExtension
    {
        private readonly IServiceProvider serviceProvider;

        private sealed class AssetTag : ArgumentsTag
        {
            private readonly IServiceProvider serviceProvider;

            public AssetTag(IServiceProvider serviceProvider)
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
                            .WithoutTotal());

                    var id = (await arguments[1].Expression.EvaluateAsync(context)).ToStringValue();

                    var assetQuery = serviceProvider.GetRequiredService<IAssetQueryService>();

                    var asset = await assetQuery.FindAsync(requestContext, DomainId.Create(id));

                    if (asset != null)
                    {
                        var name = (await arguments[0].Expression.EvaluateAsync(context)).ToStringValue();

                        context.SetValue(name, asset);
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

        public AssetsFluidExtension(IServiceProvider serviceProvider)
        {
            Guard.NotNull(serviceProvider, nameof(serviceProvider));

            this.serviceProvider = serviceProvider;
        }

        public void RegisterGlobalTypes(IMemberAccessStrategy memberAccessStrategy)
        {
            memberAccessStrategy.Register<IAssetEntity>();
            memberAccessStrategy.Register<IAssetInfo>();
            memberAccessStrategy.Register<IEntity>();
            memberAccessStrategy.Register<IEntityWithCreatedBy>();
            memberAccessStrategy.Register<IEntityWithLastModifiedBy>();
            memberAccessStrategy.Register<IEntityWithVersion>();
            memberAccessStrategy.Register<IEnrichedAssetEntity>();
        }

        public void RegisterLanguageExtensions(FluidParserFactory factory)
        {
            factory.RegisterTag("asset", new AssetTag(serviceProvider));
        }
    }
}
