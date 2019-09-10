using System;
using System.Linq;
using System.Threading.Tasks;
using DeploymentApp.Utilities;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;

namespace DeploymentApp.Extensions
{
    public delegate Task<WorkflowDto> WorkflowFactory(SquidexClientManager clientManager);

    public delegate Task SchemaSync(UpsertSchemaDto upsert);

    public delegate (string Name, SchemaSync Sync) SchemaFactory(SquidexClientManager clientManager);

    public delegate (string Name, string[] Permissions) RoleFactory();

    public delegate (string Email, string Role) ContributorFactory();

    public static class IcisClientManagerExtensions
    {
        public static async Task CreateApp(this SquidexClientManager clientManager)
        {
            var appsClient = clientManager.CreateAppsClient();
            try
            {
                ConsoleHelper.Start($"Creating app {clientManager.App}");

                await appsClient.PostAppAsync(new CreateAppDto
                {
                    Name = clientManager.App
                });

                ConsoleHelper.Success();
            }
            catch (SquidexManagementException ex)
            {
                if (ex.StatusCode.Equals(400))
                {
                    ConsoleHelper.Skipped("already exists");
                }
                else
                {
                    ConsoleHelper.Failed(ex);
                    throw;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.Failed(ex);
                throw;
            }
        }

        public static async Task UpsertSchema(this SquidexClientManager clientManager, SchemaFactory factory)
        {
            var (name, upsert) = factory(clientManager);

            var schemasClient = clientManager.CreateSchemasClient();
            try
            {
                ConsoleHelper.Start($"Creating schema {name}");

                var create = new CreateSchemaDto
                {
                    Name = name
                };

                await upsert(create);

                var response = await schemasClient.PostSchemaAsync(clientManager.App, create);

                ConsoleHelper.Success();
            }
            catch (SquidexManagementException ex)
            {
                if (ex.StatusCode.Equals(400))
                {
                    ConsoleHelper.Skipped("already exists");

                    ConsoleHelper.Start($"Syncing schema {name}");

                    var sync = new SynchronizeSchemaDto
                    {
                        NoFieldDeletion = true,
                        NoFieldRecreation = true
                    };

                    await upsert(sync);

                    await schemasClient.PutSchemaSyncAsync(clientManager.App, name, sync);

                    ConsoleHelper.Success();
                }
                else
                {
                    ConsoleHelper.Failed(ex);
                    throw;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.Failed(ex);
                throw;
            }
        }

        public static async Task UpsertRole(this SquidexClientManager clientManager, RoleFactory factory)
        {
            var (name, permissions) = factory();

            var appsClient = clientManager.CreateAppsClient();
            try
            {
                ConsoleHelper.Start($"Creating role {name}");

                await appsClient.PostRoleAsync(clientManager.App, new AddRoleDto
                {
                    Name = name
                });

                ConsoleHelper.Success();
            }
            catch (SquidexManagementException e)
            {
                if (e.StatusCode.Equals(400))
                {
                    ConsoleHelper.Skipped("Role already exists");
                }
                else
                {
                    ConsoleHelper.Failed(e);
                    throw;
                }
            }
            catch (Exception e)
            {
                ConsoleHelper.Failed(e);
                throw;
            }

            try
            {
                ConsoleHelper.Start($"Adding permissions to role {name}");

                await appsClient.PutRoleAsync(clientManager.App, name, new UpdateRoleDto
                {
                    Permissions = permissions.ToArray()
                });

                ConsoleHelper.Success();
            }
            catch (Exception e)
            {
                ConsoleHelper.Failed(e);
                throw;
            }
        }

        public static async Task UpsertLanguage(this SquidexClientManager clientManager, string languageCode)
        {
            var appsClient = clientManager.CreateAppsClient();

            try
            {
                ConsoleHelper.Start($"Creating language {languageCode}");

                await appsClient.PostLanguageAsync(clientManager.App, new AddLanguageDto
                {
                    Language = languageCode
                });

                ConsoleHelper.Success();
            }
            catch (SquidexManagementException e)
            {
                if (e.StatusCode.Equals(400))
                {
                    ConsoleHelper.Skipped("Language already exists");
                }
                else
                {
                    ConsoleHelper.Failed(e);
                    throw;
                }
            }
            catch (Exception e)
            {
                ConsoleHelper.Failed(e);
                throw;
            }
        }

        public static async Task UpsertContributor(this SquidexClientManager clientManager, ContributorFactory factory)
        {
            var (id, role) = factory();

            var appsClient = clientManager.CreateAppsClient();
            try
            {
                ConsoleHelper.Start($"Adding contributor {id} as {role}");

                await appsClient.PostContributorAsync(clientManager.App, new AssignContributorDto()
                {
                    ContributorId = id, Role = role, Invite = true
                });

                ConsoleHelper.Success();
            }
            catch (SquidexManagementException e)
            {
                if (e.StatusCode.Equals(400))
                {
                    ConsoleHelper.Skipped($"Contributor {id} already added");
                }
                else
                {
                    ConsoleHelper.Failed(e);
                    throw;
                }
            }
            catch (Exception e)
            {
                ConsoleHelper.Failed(e);
                throw;
            }
        }

        public static async Task UpsertWorkflow(this SquidexClientManager clientManager, WorkflowFactory factory)
        {
            var appsClient = clientManager.CreateAppsClient();

            var workflows = await appsClient.GetWorkflowsAsync(clientManager.App);
            var workflowDto = await factory(clientManager);

            var existingWorkflow = workflows.Items.FirstOrDefault(x => x.Name == workflowDto.Name);

            if (existingWorkflow == null)
            {
                try
                {
                    ConsoleHelper.Start($"Adding workflow {workflowDto.Name}");

                    workflows = await appsClient.PostWorkflowAsync(clientManager.App, new AddWorkflowDto()
                    {
                        Name = workflowDto.Name
                    });

                    existingWorkflow = workflows.Items.FirstOrDefault(x => x.Name == workflowDto.Name);

                    ConsoleHelper.Success();
                }
                catch (Exception e)
                {
                    ConsoleHelper.Failed(e);
                    throw;
                }
            }

            try
            {
                ConsoleHelper.Start($"Updating workflow {workflowDto.Name}");

                await appsClient.PutWorkflowAsync(clientManager.App, existingWorkflow.Id.ToString(), new UpdateWorkflowDto()
                {
                    Name = workflowDto.Name,
                    Initial = workflowDto.Initial,
                    SchemaIds = workflowDto.SchemaIds,
                    Steps = workflowDto.Steps
                });

                existingWorkflow = workflows.Items.FirstOrDefault(x => x.Name == workflowDto.Name);

                ConsoleHelper.Success();
            }
            catch (Exception e)
            {
                ConsoleHelper.Failed(e);
                throw;
            }
        }

        public static async Task CreateContentAsync(this SquidexClientManager clientManager, string schema, string id, string name)
        {
            var client = clientManager.GetClient<TestData, object>(schema);

            try
            {
                ConsoleHelper.Start($"Creating item for {schema} with id '{id}'");

                var items = await client.GetAsync(new ODataQuery
                {
                    Filter = $"data/id/iv eq '{id}'"
                }, context: QueryContext.Default.Unpublished(true));

                if (items.Total > 0)
                {
                    ConsoleHelper.Skipped("Exists");
                }
                else
                {
                    await client.CreateAsync(new
                    {
                        id = new
                        {
                            iv = id
                        },
                        name = new
                        {
                            iv = name
                        }
                    });

                    ConsoleHelper.Success();
                }
            }
            catch (Exception e)
            {
                ConsoleHelper.Failed(e);
                throw;
            }
        }

        class TestData : SquidexEntityBase<object>
        {
        }
    }
}