// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;

namespace TestSuite.Model
{
    internal static class TestClient
    {
        public const string ServerUrl = "http://localhost:5000";

        public const string ClientId = "root";
        public const string ClientSecret = "xeLd6jFxqbXJrfmNLlO2j1apagGGGSyZJhFnIuHp4I0=";

        public const string TestAppName = "integration-tests";

        public static readonly SquidexClientManager ClientManager =
            new SquidexClientManager(
                ServerUrl,
                TestAppName,
                ClientId,
                ClientSecret)
            {
                ReadResponseAsString = true
            };
    }
}