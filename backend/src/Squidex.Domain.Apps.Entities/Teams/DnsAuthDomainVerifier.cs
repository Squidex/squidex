// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using DnsClient;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Teams;

public sealed class DnsAuthDomainVerifier : IAuthDomainVerifier
{
    // Without the cache a record is found immediately after it has been added.
    private readonly LookupClient lookupClient = new LookupClient(new LookupClientOptions { UseCache = false, ThrowDnsErrors = false });

    public async Task<bool> IsVerifiedAsync(string domain, DomainId teamId,
        CancellationToken ct = default)
    {
        var expected = AuthDomainRecord.GetValue(teamId);

        try
        {
            var response = await lookupClient.QueryAsync(AuthDomainRecord.GetName(domain), QueryType.TXT, cancellationToken: ct);

            return response.Answers.TxtRecords().SelectMany(x => x.Text).Any(x => string.Equals(x.Trim(), expected, StringComparison.Ordinal));
        }
        catch (DnsResponseException)
        {
            return false;
        }
    }
}
