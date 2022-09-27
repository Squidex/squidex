// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    internal sealed class TypeNames
    {
        // Reserver names that are used for other GraphQL types.
        private static readonly HashSet<string> ReservedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Asset",
            "AssetResultDto",
            "Content",
            "Component",
            "EnrichedAssetEvent",
            "EnrichedContentEvent",
            "EntityCreatedResultDto",
            "EntitySavedResultDto",
            "JsonScalar",
            "JsonPrimitive",
            "User"
        };

        private readonly Dictionary<string, int> takenNames = new Dictionary<string, int>();

        public string this[string name, bool isEntity = true]
        {
            get => GetName(name, isEntity);
        }

        private string GetName(string name, bool isEntity)
        {
            Guard.NotNullOrEmpty(name);

            if (!char.IsLetter(name[0]))
            {
                name = "gql_" + name;
            }
            else if (isEntity && ReservedNames.Contains(name))
            {
                name = $"{name}Entity";
            }

            // Avoid duplicate names.
            if (!takenNames.TryGetValue(name, out var offset))
            {
                takenNames[name] = 0;
                return name;
            }

            takenNames[name] = ++offset;

            // Add + 1 to all offsets for backwards-compatibility.
            return $"{name}{offset + 1}";
        }
    }
}
