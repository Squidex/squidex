// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Comments;

namespace Squidex.Config.Domain
{
    public static class CommentsServices
    {
        public static void AddSquidexComments(this IServiceCollection services)
        {
            services.AddSingletonAs<CommentsLoader>()
                .As<ICommentsLoader>();
        }
    }
}