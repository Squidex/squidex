// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;

namespace TestSuite
{
    public static class ClientManagerFactory
    {
        private static Task<ClientManagerWrapper> manager;

        public static Task<ClientManagerWrapper> CreateAsync()
        {
            if (manager == null)
            {
                manager = CreateInternalAsync();
            }

            return manager;
        }

        private static async Task<ClientManagerWrapper> CreateInternalAsync()
        {
            var clientManager = new ClientManagerWrapper();

            await clientManager.ConnectAsync();

            return clientManager;
        }
    }
}
