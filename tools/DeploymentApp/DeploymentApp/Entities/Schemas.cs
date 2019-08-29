using System.Collections.Generic;
using System.Threading.Tasks;
using DeploymentApp.Extensions;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;

#pragma warning disable IDE0060

namespace DeploymentApp.Entities
{
    public class Schemas
    {
        public static (string Name, SchemaSync Sync) Commodity(SquidexClientManager clientManager)
        {
            return ("commodity", 
                upsert =>
                {
                    upsert.Fields = new List<UpsertSchemaFieldDto>
                    {
                        new UpsertSchemaFieldDto
                        {
                            Name = "id",
                            Properties = new StringFieldPropertiesDto
                            {
                                Editor = StringFieldEditor.Input,
                                IsListField = true,
                                IsRequired = true,
                                IsUnique = true,
                                Label = "Id"
                            }
                        },

                        new UpsertSchemaFieldDto
                        {
                            Name = "name",
                            Properties = new StringFieldPropertiesDto
                            {
                                Editor = StringFieldEditor.Input,
                                IsListField = true,
                                IsReferenceField = true,
                                IsRequired = true,
                                IsUnique = true,
                                Label = "Name"
                            }
                        }
                    };

                    upsert.IsPublished = true;

                    return Task.CompletedTask;
                });
        }

        public static (string Name, SchemaSync Sync) Region(SquidexClientManager clientManager)
        {
            return ("region", 
                upsert =>
                {
                    upsert.Fields = new List<UpsertSchemaFieldDto>
                    {
                        new UpsertSchemaFieldDto
                        {
                            Name = "id",
                            Properties = new StringFieldPropertiesDto
                            {
                                Editor = StringFieldEditor.Input,
                                IsListField = true,
                                IsRequired = true,
                                IsUnique = true,
                                Label = "Id"
                            }
                        },

                        new UpsertSchemaFieldDto
                        {
                            Name = "name",
                            Properties = new StringFieldPropertiesDto
                            {
                                Editor = StringFieldEditor.Input,
                                IsListField = true,
                                IsReferenceField = true,
                                IsRequired = true,
                                IsUnique = true,
                                Label = "Name"
                            }
                        }
                    };

                    upsert.IsPublished = true;

                    return Task.CompletedTask;
                }
            );
        }

        public static (string Name, SchemaSync Sync) CommentaryType(SquidexClientManager clientManager)
        {
            return ("commentary-type",
                upsert =>
                {
                    upsert.Fields = new List<UpsertSchemaFieldDto>
                    {
                        new UpsertSchemaFieldDto
                        {
                            Name = "id",
                            Properties = new StringFieldPropertiesDto
                            {
                                Editor = StringFieldEditor.Input,
                                IsListField = true,
                                IsRequired = true,
                                IsUnique = true,
                                Label = "Id"
                            }
                        },

                        new UpsertSchemaFieldDto
                        {
                            Name = "name",
                            Properties = new StringFieldPropertiesDto
                            {
                                Editor = StringFieldEditor.Input,
                                IsListField = true,
                                IsReferenceField = true,
                                IsRequired = true,
                                IsUnique = true,
                                Label = "Name"
                            }
                        }
                    };

                    upsert.IsPublished = true;

                    return Task.CompletedTask;
                }
            );
        }

        public static SchemaFactory Commentary(string baseUrl)
        {
            return (SquidexClientManager clientManager) =>
            {
                return ("commentary",
                    async upsert =>
                    {
                        var schemasClient = clientManager.CreateSchemasClient();
                        var commoditySchema = await schemasClient.GetSchemaAsync(clientManager.App, "commodity");
                        var regionSchema = await schemasClient.GetSchemaAsync(clientManager.App, "region");
                        var commentaryTypeSchema = await schemasClient.GetSchemaAsync(clientManager.App, "commentary-type");

                        upsert.Fields = new List<UpsertSchemaFieldDto>
                        {
                            new UpsertSchemaFieldDto
                            {
                                Name = "createdfor",
                                Partitioning = "invariant",
                                Properties = new DateTimeFieldPropertiesDto()
                                {
                                    CalculatedDefaultValue = DateTimeCalculatedDefaultValue.Today,
                                    Editor = DateTimeFieldEditor.Date,
                                    IsListField = true,
                                    IsReferenceField = false,
                                    IsRequired = false,
                                    Label = "Created For Date"
                                }
                            },

                            new UpsertSchemaFieldDto
                            {
                                Name = "commodity",
                                Partitioning = "invariant",
                                Properties = new ReferencesFieldPropertiesDto()
                                {
                                    Editor = ReferencesFieldEditor.Dropdown,
                                    IsListField = true,
                                    IsReferenceField = true,
                                    IsRequired = true,
                                    Label = "Commodity",
                                    MaxItems = 1,
                                    MinItems = 1,
                                    ResolveReference = true,
                                    SchemaId = commoditySchema.Id
                                }
                            },

                            new UpsertSchemaFieldDto
                            {
                                Name = "commentarytype",
                                Partitioning = "invariant",
                                Properties = new ReferencesFieldPropertiesDto()
                                {
                                    Editor = ReferencesFieldEditor.Dropdown,
                                    IsListField = true,
                                    IsRequired = true,
                                    Label = "Commentary Type",
                                    MaxItems = 1,
                                    MinItems = 1,
                                    ResolveReference = true,
                                    SchemaId = commentaryTypeSchema.Id
                                }
                            },

                            new UpsertSchemaFieldDto
                            {
                                Name = "region",
                                Partitioning = "invariant",
                                Properties = new ReferencesFieldPropertiesDto()
                                {
                                    Editor = ReferencesFieldEditor.Dropdown,
                                    IsListField = true,
                                    IsRequired = true,
                                    Label = "Region",
                                    MaxItems = 1,
                                    MinItems = 1,
                                    ResolveReference = true,
                                    SchemaId = regionSchema.Id
                                }
                            },

                            new UpsertSchemaFieldDto
                            {
                                Name = "body",
                                Partitioning = "language",
                                Properties = new StringFieldPropertiesDto()
                                {
                                    Editor = StringFieldEditor.Input,
                                    EditorUrl = $"{baseUrl}/editors/toastui/md-editor.html",
                                    IsListField = false,
                                    IsRequired = false,
                                    IsUnique = false,
                                    Label = "Body",
                                }
                            }
                        };

                        upsert.IsPublished = true;
                    }
                );
            };
        }
    }
}

#pragma warning restore IDE0060