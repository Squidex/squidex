// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Email
{
    public interface IEmailSender
    {
        Task SendAsync(string recipient, string subject, string body,
            CancellationToken ct = default);
    }
}
