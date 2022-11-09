// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;

namespace Squidex.Infrastructure.MongoDb;

public static class Field
{
    public static string Of<T>(Func<T, string> mapper)
    {
        var name = mapper(default!);

        var classMap = BsonClassMap.LookupClassMap(typeof(T));

        // The class map does not contain all inherited members, therefore we have to loop over the hierarchy.
        while (classMap != null)
        {
            var member = classMap.GetMemberMap(name);

            if (member != null)
            {
                return member.ElementName;
            }

            classMap = classMap.BaseClassMap;
        }

        ThrowHelper.InvalidOperationException($"Cannot find member '{name}' in type '{typeof(T)}.");
        return null!;
    }
}
