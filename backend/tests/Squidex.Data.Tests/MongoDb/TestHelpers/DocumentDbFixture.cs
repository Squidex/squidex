// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.TestHelpers;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.MongoDb.TestHelpers;

[CollectionDefinition(Name)]
public sealed class DocumentDbCollection : ICollectionFixture<DocumentDbFixture>
{
    public const string Name = "DocumentDb";
}

public class DocumentDbFixture
{
    public IMongoClient Client { get; private set; }

    public IMongoDatabase Database => Client.GetDatabase($"Test_{Guid.NewGuid()}");

    public DocumentDbFixture()
    {
        MongoTestUtils.SetupBson();

        var settings = MongoClientSettings.FromConnectionString(
            TestConfig.Configuration.GetValue<string>("documentDb:configuration")
        );

        var certPath = TestConfig.Configuration.GetValue<string>("documentDb:keyFile")!;
        var certFile = new X509Certificate2(certPath);

        settings.RetryWrites = false;
        settings.RetryReads = false;
        settings.SslSettings = new SslSettings
        {
            ClientCertificates = [certFile],
            CheckCertificateRevocation = false,
            ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true,
        };

        Client = new MongoClient(settings);
    }
}
