// ==========================================================================
//  IUserEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Read.Users
{
    public interface IUserEntity
    {
        string Id { get; }

        string Email { get; }

        string PictureUrl { get; }

        string DisplayName { get; }

        bool IsLocked { get; }
    }
}
