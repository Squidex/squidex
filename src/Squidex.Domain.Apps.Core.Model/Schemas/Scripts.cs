// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using System.Collections.Generic;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public static class Scripts
    {
        public const string Change = "Change";
        public const string Create = "Create";
        public const string Query = "Query";
        public const string Update = "Update";
        public const string Delete = "Delete";

        public static string GetChange(this IReadOnlyDictionary<string, string> scripts)
        {
            return scripts?.GetOrDefault(Change);
        }

        public static string GetCreate(this IReadOnlyDictionary<string, string> scripts)
        {
            return scripts?.GetOrDefault(Create);
        }

        public static string GetQuery(this IReadOnlyDictionary<string, string> scripts)
        {
            return scripts?.GetOrDefault(Query);
        }

        public static string GetUpdate(this IReadOnlyDictionary<string, string> scripts)
        {
            return scripts?.GetOrDefault(Update);
        }

        public static string GetDelete(this IReadOnlyDictionary<string, string> scripts)
        {
            return scripts?.GetOrDefault(Delete);
        }
    }
}
