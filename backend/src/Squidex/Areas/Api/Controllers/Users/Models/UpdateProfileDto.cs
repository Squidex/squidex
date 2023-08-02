// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Users;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Users.Models;

[OpenApiRequest]
public sealed class UpdateProfileDto
{
    /// <summary>
    /// The answers from a questionaire.
    /// </summary>
    public Dictionary<string, string>? Answers { get; set; }

    public UserValues ToValues()
    {
        return new UserValues
        {
            Answers = Answers
        };
    }
}
