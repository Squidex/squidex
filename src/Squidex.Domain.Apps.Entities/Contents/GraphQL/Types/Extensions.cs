// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.DataLoader;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public static class Extensions
    {
        public static IEnumerable<(T Field, string Name, string Type)> SafeFields<T>(this IEnumerable<T> fields) where T : IField
        {
            var allFields =
                fields.ForApi()
                    .Select(f => (Field: f, Name: f.Name.ToCamelCase(), Type: f.TypeName())).GroupBy(x => x.Name)
                    .Select(g =>
                    {
                        return g.Select((f, i) => (f.Field, f.Name.SafeString(i), f.Type.SafeString(i)));
                    })
                    .SelectMany(x => x);

            return allFields;
        }

        private static string SafeString(this string value, int index)
        {
            if (index > 0)
            {
                return value + (index + 1);
            }

            return value;
        }

        public static async Task<IReadOnlyList<T>> LoadManyAsync<TKey, T>(this IDataLoader<TKey, T> dataLoader, ICollection<TKey> keys) where T : class
        {
            var contents = await Task.WhenAll(keys.Select(x => dataLoader.LoadAsync(x)));

            return contents.Where(x => x != null).ToList();
        }
    }
}
