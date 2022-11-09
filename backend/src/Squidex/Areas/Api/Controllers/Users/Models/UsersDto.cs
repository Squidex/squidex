// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Validation;
using Squidex.Shared.Users;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Users.Models;

public sealed class UsersDto : Resource
{
    /// <summary>
    /// The total number of users.
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// The users.
    /// </summary>
    [LocalizedRequired]
    public UserDto[] Items { get; set; }

    public static UsersDto FromDomain(IEnumerable<IUser> items, long total, Resources resources)
    {
        var result = new UsersDto
        {
            Total = total,
            Items = items.Select(x => UserDto.FromDomain(x, resources)).ToArray()
        };

        return result.CreateLinks(resources);
    }

    private UsersDto CreateLinks(Resources context)
    {
        AddSelfLink(context.Url<UserManagementController>(c => nameof(c.GetUsers)));

        if (context.CanCreateUser)
        {
            AddPostLink("create",
                context.Url<UserManagementController>(c => nameof(c.PostUser)));
        }

        return this;
    }
}
