// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;

public sealed class ReservedNames
{
    private readonly Dictionary<string, int> takenNames;

    public string this[string name]
    {
        get => GetName(name);
    }

    private ReservedNames(Dictionary<string, int> takenNames)
    {
        this.takenNames = takenNames;
    }

    public static ReservedNames ForFields()
    {
        var reserved = new Dictionary<string, int>();

        return new ReservedNames(reserved);
    }

    public static ReservedNames ForTypes()
    {
        // Reserver names that are used for other GraphQL types.
        var reserved = new Dictionary<string, int>
        {
            ["Asset"] = 1,
            ["AssetResultDto"] = 1,
            ["Content"] = 1,
            ["Component"] = 1,
            ["EnrichedAssetEvent"] = 1,
            ["EnrichedContentEvent"] = 1,
            ["EntityCreatedResultDto"] = 1,
            ["EntitySavedResultDto"] = 1,
            ["JsonObject"] = 1,
            ["JsonScalar"] = 1,
            ["JsonPrimitive"] = 1,
            ["User"] = 1,
        };

        return new ReservedNames(reserved);
    }

    private string GetName(string name)
    {
        Guard.NotNullOrEmpty(name);

        if (!char.IsLetter(name[0]))
        {
            name = "gql_" + name;
        }

        if (!takenNames.TryGetValue(name, out var offset))
        {
            // If the name is free, we do not add an offset.
            takenNames[name] = 1;

            return name;
        }
        else
        {
            // Add + 1 to all offsets for backwards-compatibility.
            takenNames[name] = ++offset;

            return $"{name}{offset}";
        }
    }
}
