using System;
using System.Linq;
using System.Threading.Tasks;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;

namespace DeploymentApp.Extensions
{
    public delegate Task<WorkflowDto> WorkflowFactory(SquidexClientManager clientManager);

    public delegate Task SchemaSync(UpsertSchemaDto upsert);

    public delegate (string Name, SchemaSync Sync) SchemaFactory(SquidexClientManager clientManager);

    public delegate (string Name, string[] Permissions) RoleFactory();

    public delegate (string Email, string Role) ContributorFactory();

    public sealed class IcisClient
    {
        public SquidexClientManager ClientManager { get; }

        public ILogger Log { get; }

        public IcisClient(ILogger logger, SquidexClientManager clientManager)
        {
            Log = logger;

            ClientManager = clientManager;
        }

        public async Task CreateApp()
        {
            var appsClient = ClientManager.CreateAppsClient();
            try
            {
                Log.Start($"Creating app {ClientManager.App}");

                await appsClient.PostAppAsync(new CreateAppDto
                {
                    Name = ClientManager.App
                });

                Log.Success();
            }
            catch (SquidexManagementException ex)
            {
                if (ex.StatusCode.Equals(400))
                {
                    Log.Skipped("already exists");
                }
                else
                {
                    Log.Failed(ex);
                    throw;
                }
            }
            catch (Exception ex)
            {
                Log.Failed(ex);
                throw;
            }
        }

        public async Task UpsertSchema(SchemaFactory factory)
        {
            var (name, upsert) = factory(ClientManager);

            var schemasClient = ClientManager.CreateSchemasClient();
            try
            {
                Log.Start($"Creating schema {name}");

                var create = new CreateSchemaDto
                {
                    Name = name
                };

                await upsert(create);

                var response = await schemasClient.PostSchemaAsync(ClientManager.App, create);

                Log.Success();
            }
            catch (SquidexManagementException ex)
            {
                if (ex.StatusCode.Equals(400))
                {
                    Log.Skipped("already exists");

                    Log.Start($"Syncing schema {name}");

                    var sync = new SynchronizeSchemaDto 
                    {
                        NoFieldDeletion = true,
                        NoFieldRecreation = false
                    };

                    await upsert(sync);

                    await schemasClient.PutSchemaSyncAsync(ClientManager.App, name, sync);

                    Log.Success();
                }
                else
                {
                    Log.Failed(ex);
                    throw;
                }
            }
            catch (Exception ex)
            {
                Log.Failed(ex);
                throw;
            }
        }

        public async Task UpsertRole(RoleFactory factory)
        {
            var (name, permissions) = factory();

            var appsClient = ClientManager.CreateAppsClient();
            try
            {
                Log.Start($"Creating role {name}");

                await appsClient.PostRoleAsync(ClientManager.App, new AddRoleDto
                {
                    Name = name
                });

                Log.Success();
            }
            catch (SquidexManagementException e)
            {
                if (e.StatusCode.Equals(400))
                {
                    Log.Skipped("Role already exists");
                }
                else
                {
                    Log.Failed(e);
                    throw;
                }
            }
            catch (Exception e)
            {
                Log.Failed(e);
                throw;
            }

            try
            {
                Log.Start($"Adding permissions to role {name}");

                await appsClient.PutRoleAsync(ClientManager.App, name, new UpdateRoleDto
                {
                    Permissions = permissions.ToArray()
                });

                Log.Success();
            }
            catch (Exception e)
            {
                Log.Failed(e);
                throw;
            }
        }

        public async Task UpsertLanguage(string languageCode)
        {
            var appsClient = ClientManager.CreateAppsClient();

            try
            {
                Log.Start($"Creating language {languageCode}");

                await appsClient.PostLanguageAsync(ClientManager.App, new AddLanguageDto
                {
                    Language = languageCode
                });

                Log.Success();
            }
            catch (SquidexManagementException e)
            {
                if (e.StatusCode.Equals(400))
                {
                    Log.Skipped("Language already exists");
                }
                else
                {
                    Log.Failed(e);
                    throw;
                }
            }
            catch (Exception e)
            {
                Log.Failed(e);
                throw;
            }

            try
            {
                Log.Start($"Updating language {languageCode}");

                await appsClient.PutLanguageAsync(ClientManager.App, languageCode, new UpdateLanguageDto
                {
                    IsOptional = true
                });

                Log.Success();
            }
            catch (Exception e)
            {
                Log.Failed(e);
                throw;
            }
        }

        public async Task UpsertContributor(ContributorFactory factory)
        {
            var (id, role) = factory();

            var appsClient = ClientManager.CreateAppsClient();
            try
            {
                Log.Start($"Adding contributor {id} as {role}");

                await appsClient.PostContributorAsync(ClientManager.App, new AssignContributorDto()
                {
                    ContributorId = id, Role = role, Invite = true
                });

                Log.Success();
            }
            catch (SquidexManagementException e)
            {
                if (e.StatusCode.Equals(400))
                {
                    Log.Skipped($"Contributor {id} already added");
                }
                else
                {
                    Log.Failed(e);
                    throw;
                }
            }
            catch (Exception e)
            {
                Log.Failed(e);
                throw;
            }
        }

        public async Task UpsertWorkflow(WorkflowFactory factory)
        {
            var appsClient = ClientManager.CreateAppsClient();

            var workflows = await appsClient.GetWorkflowsAsync(ClientManager.App);
            var workflowDto = await factory(ClientManager);

            var existingWorkflow = workflows.Items.FirstOrDefault(x => x.Name == workflowDto.Name);

            if (existingWorkflow == null)
            {
                try
                {
                    Log.Start($"Adding workflow {workflowDto.Name}");

                    workflows = await appsClient.PostWorkflowAsync(ClientManager.App, new AddWorkflowDto()
                    {
                        Name = workflowDto.Name
                    });

                    existingWorkflow = workflows.Items.FirstOrDefault(x => x.Name == workflowDto.Name);

                    Log.Success();
                }
                catch (Exception e)
                {
                    Log.Failed(e);
                    throw;
                }
            }

            try
            {
                Log.Start($"Updating workflow {workflowDto.Name}");

                await appsClient.PutWorkflowAsync(ClientManager.App, existingWorkflow.Id.ToString(), new UpdateWorkflowDto()
                {
                    Name = workflowDto.Name,
                    Initial = workflowDto.Initial,
                    SchemaIds = workflowDto.SchemaIds,
                    Steps = workflowDto.Steps
                });

                existingWorkflow = workflows.Items.FirstOrDefault(x => x.Name == workflowDto.Name);

                Log.Success();
            }
            catch (Exception e)
            {
                Log.Failed(e);
                throw;
            }
        }

        public async Task CreateIdDataAsync(string schema, string id, object data, bool isLocalized)
        {
            var client = ClientManager.GetClient<TestData, object>(schema);

            try
            {
                Log.Start($"Creating item for {schema} with id '{id}'");

                var items = await client.GetAsync(new ODataQuery
                {
                    Filter = $"data/id/iv eq '{id}'"
                }, context: QueryContext.Default.Unpublished(true));

                if (items.Total > 0)
                {
                    Log.Skipped("Exists");
                }
                else
                {
                    if (data is string name)
                    {
                        var content = new Object();

                        if (isLocalized)
                        {
                            content = new
                            {
                                id = new
                                {
                                    iv = id
                                },
                                name = new
                                {
                                    en = name
                                }
                            };
                        }
                        else
                        {
                            content = new
                            {
                                id = new
                                {
                                    iv = id
                                },
                                name = new
                                {
                                    iv = name
                                }
                            };
                        }
                        
                        await client.CreateAsync(content);
                    }
                    else
                    {
                        await client.CreateAsync(data);
                    }

                    Log.Success();
                }
            }
            catch (Exception e)
            {
                Log.Failed(e);
                throw;
            }
        }

        public async Task CreateCommentaryAsync(string createFor, string commodity, string commentaryType, string region, string body)
        {
            var commentaries = ClientManager.GetClient<TestData, object>("commentary");

            try
            {
                Log.Start($"Creating commentary");

                var commodityId = await GetReferenceAsync("commodity", commodity);
                var commentaryTypeId = await GetReferenceAsync("commentary-type", commentaryType);
                var regionId = await GetReferenceAsync("region", region);

                await commentaries.CreateAsync(new
                {
                    createdfor = new
                    {
                        iv = createFor
                    },
                    commodity = new
                    {
                        iv = new[] { commodityId }
                    },
                    commentarytype = new
                    {
                        iv = new[] { commentaryTypeId }
                    },
                    region = new
                    {
                        iv = new[] { regionId }
                    },
                    body = new
                    {
                        en = body
                    }
                });

                Log.Success();
            }
            catch (Exception e)
            {
                Log.Failed(e);
                throw;
            }
        }

        private async Task<string> GetReferenceAsync(string schema, string id)
        {
            using (var client = ClientManager.GetClient<TestData, object>(schema))
            {
                var items = await client.GetAsync(new ODataQuery
                {
                    Filter = $"data/id/iv eq '{id}'",
                    Top = 1
                }, context: QueryContext.Default.Unpublished(true));

                if (items.Total == 0)
                {
                    throw new InvalidOperationException($"Cannot find entry for schema: {schema}");
                }

                return items.Items[0].Id;
            }
        }

        private sealed class TestData : SquidexEntityBase<object>
        {
        }
    }
}