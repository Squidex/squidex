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
using Fluid.Values;
using GraphQL.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Templates;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetsFluidExtension : IFluidExtension
    {
        private static readonly FluidValue ErrorNullAsset = FluidValue.Create(null);
        private static readonly FluidValue ErrorNoAsset = new StringValue("NoAsset");
        private static readonly FluidValue ErrorTooBig = new StringValue("ErrorTooBig");
        private readonly IServiceProvider serviceProvider;

        private sealed class AssetTag : AppTag
        {
            private readonly IAssetQueryService assetQuery;

            public AssetTag(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
                assetQuery = serviceProvider.GetRequiredService<IAssetQueryService>();
            }

            public override async ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, FilterArgument[] arguments)
            {
                if (arguments.Length == 2 && context.GetValue("event")?.ToObjectValue() is EnrichedEvent enrichedEvent)
                {
                    var id = await arguments[1].Expression.EvaluateAsync(context);

                    var content = await ResolveAssetAsync(AppProvider, assetQuery, enrichedEvent.AppId.Id, id);

                    if (content != null)
                    {
                        var name = (await arguments[0].Expression.EvaluateAsync(context)).ToStringValue();

                        context.SetValue(name, content);
                    }
                }

                return Completion.Normal;
            }
        }

        public AssetsFluidExtension(IServiceProvider serviceProvider)
        {
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

            AddAssetFilter();
            AddAssetTextFilter();
        }

        private void AddAssetFilter()
        {
            var appProvider = serviceProvider.GetRequiredService<IAppProvider>();

            var assetQuery = serviceProvider.GetRequiredService<IAssetQueryService>();

            TemplateContext.GlobalFilters.AddAsyncFilter("asset", async (input, arguments, context) =>
            {
                if (context.GetValue("event")?.ToObjectValue() is EnrichedEvent enrichedEvent)
                {
                    var asset = await ResolveAssetAsync(appProvider, assetQuery, enrichedEvent.AppId.Id, input);

                    if (asset == null)
                    {
                        return ErrorNullAsset;
                    }

                    return FluidValue.Create(asset);
                }

                return ErrorNullAsset;
            });
        }

        private void AddAssetTextFilter()
        {
            var assetFileStore = serviceProvider.GetRequiredService<IAssetFileStore>();

            TemplateContext.GlobalFilters.AddAsyncFilter("assetText", async (input, arguments, context) =>
            {
                if (input is not ObjectValue objectValue)
                {
                    return ErrorNoAsset;
                }

                async Task<FluidValue> ResolveAssetText(DomainId appId, DomainId id, long fileSize, long fileVersion)
                {
                    if (fileSize > 256_000)
                    {
                        return ErrorTooBig;
                    }

                    var encoding = arguments.At(0).ToStringValue()?.ToUpperInvariant();
                    var encoded = await assetFileStore.GetTextAsync(appId, id, fileVersion, encoding);

                    return new StringValue(encoded);
                }

                switch (objectValue.ToObjectValue())
                {
                    case IAssetEntity asset:
                        return await ResolveAssetText(asset.AppId.Id, asset.Id, asset.FileSize, asset.FileVersion);

                    case EnrichedAssetEvent @event:
                        return await ResolveAssetText(@event.AppId.Id, @event.Id, @event.FileSize, @event.FileVersion);
                }

                return ErrorNoAsset;
            });
        }

        public void RegisterLanguageExtensions(FluidParserFactory factory)
        {
            factory.RegisterTag("asset", new AssetTag(serviceProvider));
        }

        private static async Task<IAssetEntity?> ResolveAssetAsync(IAppProvider appProvider, IAssetQueryService assetQuery, DomainId appId, FluidValue id)
        {
            var app = await appProvider.GetAppAsync(appId);

            if (app == null)
            {
                return null;
            }

            var domainId = DomainId.Create(id.ToStringValue());

            var requestContext =
                Context.Admin(app).Clone(b => b
                    .WithoutTotal());

            var asset = await assetQuery.FindAsync(requestContext, domainId);

            return asset;
        }
    }
}
