// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    internal sealed class GeoQueryTransformer : AsyncTransformVisitor<ClrValue, GeoQueryTransformer.Args>
    {
        public static readonly GeoQueryTransformer Instance = new GeoQueryTransformer();

        public readonly struct Args
        {
            public readonly ITextIndex TextIndex;

            public readonly ISchemaEntity Schema;

            public readonly Context Context;

            public Args(Context context, ISchemaEntity schema, ITextIndex textIndex)
            {
                Context = context;
                Schema = schema;
                TextIndex = textIndex;
            }
        }

        private GeoQueryTransformer()
        {
        }

        public static async Task<FilterNode<ClrValue>?> TransformAsync(FilterNode<ClrValue> filter, Context context, ISchemaEntity schema, ITextIndex textIndex)
        {
            var args = new Args(context, schema, textIndex);

            return await filter.Accept(Instance, args);
        }

        public override async ValueTask<FilterNode<ClrValue>?> Visit(CompareFilter<ClrValue> nodeIn, Args args)
        {
            if (nodeIn.Value.Value is FilterSphere sphere)
            {
                var field = string.Join(".", nodeIn.Path.Skip(1));

                var searchQuery = new GeoQuery(args.Schema.Id, field, sphere.Latitude, sphere.Longitude, sphere.Radius);
                var searchScope = args.Context.Scope();

                var ids = await args.TextIndex.SearchAsync(args.Context.App, searchQuery, searchScope);

                if (ids == null || ids.Count == 0)
                {
                    return ClrFilter.Eq("id", "__notfound__");
                }

                return ClrFilter.In("id", ids.Select(x => x.ToString()).ToList());
            }

            return nodeIn;
        }
    }
}
