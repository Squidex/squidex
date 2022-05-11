// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;
using TestSuite.Model;

namespace TestSuite
{
    public static class SharedInstances
    {
        private static readonly string[] Contributors =
        {
            "hello@squidex.io"
        };

        private static readonly Task<ClientManagerWrapper> ClientManager = CreateClientManagerInternalAsync();

        private static readonly ConcurrentDictionary<string, Task> ReferenceSchemas =
            new ConcurrentDictionary<string, Task>();

        private static readonly ConcurrentDictionary<string, Task> DefaultSchemas =
            new ConcurrentDictionary<string, Task>();

        private static readonly Task App = CreateAppInternalAsync();

        private static Task<ClientManagerWrapper> CreateClientManagerInternalAsync()
        {
            var clientManager = new ClientManagerWrapper();

            return clientManager.ConnectAsync();
        }

        private static async Task CreateAppInternalAsync()
        {
            var wrapper = await ClientManager;

            try
            {
                await wrapper.Apps.PostAppAsync(new CreateAppDto { Name = wrapper.ClientManager.App });
            }
            catch (SquidexManagementException ex)
            {
                if (ex.StatusCode != 400)
                {
                    throw;
                }
            }

            var invite = new AssignContributorDto { Invite = true, Role = "Owner" };

            foreach (var contributor in Contributors)
            {
                invite.ContributorId = contributor;

                await wrapper.Apps.PostContributorAsync(wrapper.ClientManager.App, invite);
            }

            try
            {
                await wrapper.Apps.PostLanguageAsync(wrapper.ClientManager.App, new AddLanguageDto
                {
                    Language = "de"
                });
            }
            catch (SquidexManagementException ex)
            {
                if (ex.StatusCode != 400)
                {
                    throw;
                }
            }
        }

        public static Task<ClientManagerWrapper> CreateClientManagerAsync()
        {
            return ClientManager;
        }

        public static async Task<IContentsClient<TestEntityWithReferences, TestEntityWithReferencesData>> CreateReferenceSchema(string name)
        {
            var wrapper = await ClientManager;

            async Task CreateAsync()
            {
                try
                {
                    await TestEntityWithReferences.CreateSchemaAsync(wrapper.Schemas, wrapper.ClientManager.App, name);
                }
                catch (SquidexManagementException ex)
                {
                    if (ex.StatusCode != 400)
                    {
                        throw;
                    }
                }
            }

            await ReferenceSchemas.GetOrAdd(name, _ => CreateAsync());

            return wrapper.ClientManager.CreateContentsClient<TestEntityWithReferences, TestEntityWithReferencesData>(name);
        }

        public static async Task<IContentsClient<TestEntity, TestEntityData>> CreateDefaultSchema(string name)
        {
            var wrapper = await ClientManager;

            async Task CreateAsync()
            {
                try
                {
                    await TestEntity.CreateSchemaAsync(wrapper.Schemas, wrapper.ClientManager.App, name);
                }
                catch (SquidexManagementException ex)
                {
                    if (ex.StatusCode != 400)
                    {
                        throw;
                    }
                }
            }

            await DefaultSchemas.GetOrAdd(name, _ => CreateAsync());

            return wrapper.ClientManager.CreateContentsClient<TestEntity, TestEntityData>(name);
        }
    }
}
