// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Squidex.Domain.Users;

internal static class MongoIdentityClassMap
{
    public static void RegisterClassMap()
    {
        BsonClassMap.RegisterClassMap<IdentityRole<string>>(cm =>
        {
            cm.AutoMap();

            cm.MapMember(x => x.Id)
                .SetSerializer(new StringSerializer(BsonType.ObjectId));

            cm.UnmapMember(x => x.ConcurrencyStamp);
        });

        BsonClassMap.RegisterClassMap<Claim>(cm =>
        {
            cm.MapConstructor(typeof(Claim).GetConstructors()
                .First(x =>
                {
                    var parameters = x.GetParameters();

                    return parameters.Length == 2 &&
                        parameters[0].Name == "type" &&
                        parameters[0].ParameterType == typeof(string) &&
                        parameters[1].Name == "value" &&
                        parameters[1].ParameterType == typeof(string);
                }))
                .SetArguments(
                [
                    nameof(Claim.Type),
                    nameof(Claim.Value),
                ]);

            cm.MapMember(x => x.Type);
            cm.MapMember(x => x.Value);
        });

        BsonClassMap.RegisterClassMap<UserLogin>(cm =>
        {
            cm.MapConstructor(typeof(UserLogin).GetConstructors()
                .First(x =>
                {
                    var parameters = x.GetParameters();

                    return parameters.Length == 3;
                }))
                .SetArguments(
                [
                    nameof(UserLogin.LoginProvider),
                    nameof(UserLogin.ProviderKey),
                    nameof(UserLogin.ProviderDisplayName),
                ]);

            cm.AutoMap();
        });

        BsonClassMap.RegisterClassMap<IdentityUserToken<string>>(cm =>
        {
            cm.AutoMap();

            cm.UnmapMember(x => x.UserId);
        });

        BsonClassMap.RegisterClassMap<IdentityUser<string>>(cm =>
        {
            cm.AutoMap();

            cm.MapMember(x => x.Id)
                .SetSerializer(new StringSerializer(BsonType.ObjectId));

            cm.MapMember(x => x.AccessFailedCount)
                .SetIgnoreIfDefault(true);

            cm.MapMember(x => x.EmailConfirmed)
                .SetIgnoreIfDefault(true);

            cm.MapMember(x => x.LockoutEnd)
                .SetElementName("LockoutEndDateUtc").SetIgnoreIfNull(true);

            cm.MapMember(x => x.LockoutEnabled)
                .SetIgnoreIfDefault(true);

            cm.MapMember(x => x.PasswordHash)
                .SetIgnoreIfNull(true);

            cm.MapMember(x => x.PhoneNumber)
                .SetIgnoreIfNull(true);

            cm.MapMember(x => x.PhoneNumberConfirmed)
                .SetIgnoreIfDefault(true);

            cm.MapMember(x => x.SecurityStamp)
                .SetIgnoreIfNull(true);

            cm.MapMember(x => x.TwoFactorEnabled)
                .SetIgnoreIfDefault(true);
        });

        BsonSerializer.RegisterSerializer(new IdentityUserForwardingSerializer());
    }
}
