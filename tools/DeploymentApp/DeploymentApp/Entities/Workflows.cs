using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeploymentApp.Extensions;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;

namespace DeploymentApp.Entities
{
    public class Workflows
    {
        public static readonly WorkflowFactory[] All =
        {
            CommentaryWorkflow,
            RefDataWorkflow
        };

        public static async Task<WorkflowDto> CommentaryWorkflow(SquidexClientManager clientManager)
        {
            var workflowDto = new WorkflowDto
            {
                Name = "ICIS Commentary Workflow"
            };

            workflowDto.Initial = "Draft";
            workflowDto.SchemaIds = await GetSchemaIds(clientManager, "commentary", "commentary-type");
            workflowDto.Steps = new Dictionary<string, WorkflowStepDto>()
            {
                {
                    "Draft",
                    new WorkflowStepDto()
                    {
                        Color = "#8091a5",
                        Transitions = new Dictionary<string, WorkflowTransitionDto>()
                        {
                            {"In Progress", new WorkflowTransitionDto()}
                        },
                        NoUpdate = false
                    }
                },
                {
                    "In Progress",
                    new WorkflowStepDto()
                    {
                        Color = "#17e6dd",
                        Transitions = new Dictionary<string, WorkflowTransitionDto>()
                        {
                            {"In Review", new WorkflowTransitionDto()}
                        },
                        NoUpdate = false
                    }
                },
                {
                    "Correction",
                    new WorkflowStepDto()
                    {
                        Color = "#e61717",
                        Transitions = new Dictionary<string, WorkflowTransitionDto>()
                        {
                            {"Published", new WorkflowTransitionDto()}
                        },
                        NoUpdate = false
                    }
                },
                {
                    "In Review",
                    new WorkflowStepDto()
                    {
                        Color = "#c5e617",
                        Transitions = new Dictionary<string, WorkflowTransitionDto>()
                        {
                            {"In Progress", new WorkflowTransitionDto()},
                            {"Published", new WorkflowTransitionDto() }
                        },
                        NoUpdate = false
                    }
                },
                {
                    "Published",
                    new WorkflowStepDto()
                    {
                        Color = "#4bb958",
                        Transitions = new Dictionary<string, WorkflowTransitionDto>()
                        {
                            {"Correction", new WorkflowTransitionDto()}
                        },
                        NoUpdate = false
                    }
                }
            };

            return workflowDto;

        }

        public static async Task<WorkflowDto> RefDataWorkflow(SquidexClientManager clientManager)
        {
            var workflowDto = new WorkflowDto
            {
                Name = "ICIS RefData Workflow"
            };

            workflowDto.Initial = "Draft";
            workflowDto.SchemaIds = await GetSchemaIds(clientManager, "commodity", "region");
            workflowDto.Steps = new Dictionary<string, WorkflowStepDto>()
            {
                {
                    "Draft",
                    new WorkflowStepDto()
                    {
                        Color = "#8091a5",
                        Transitions = new Dictionary<string, WorkflowTransitionDto>()
                        {
                            {"Published", new WorkflowTransitionDto()}
                        },
                        NoUpdate = false
                    }
                },
                {
                    "Published",
                    new WorkflowStepDto()
                    {
                        Color = "#4bb958",
                        Transitions = new Dictionary<string, WorkflowTransitionDto>()
                        {
                            {"Draft", new WorkflowTransitionDto()}
                        },
                        NoUpdate = false
                    }
                }
            };

            return workflowDto;
        }

        private static async Task<List<Guid>> GetSchemaIds(SquidexClientManager clientManager, params string[] schemaNames)
        {
            var schemasClient = clientManager.CreateSchemasClient();
            var schemas = await schemasClient.GetSchemasAsync(clientManager.App);

            var schemaIds = new List<Guid>();

            foreach (var schemaName in schemaNames)
            {
                var schema = schemas.Items.FirstOrDefault(x => x.Name == schemaName);

                if (schema != null)
                {
                    schemaIds.Add(schema.Id);
                }
            }

            return schemaIds;
        }
    }
}